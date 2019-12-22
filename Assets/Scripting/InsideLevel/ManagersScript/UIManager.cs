﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region VARIABLES

	[Header("HUD")]

    [SerializeField]
    private GameObject endTurnButton;

	[SerializeField]
	private GameObject optionsScreen;

	[SerializeField]
	private GameObject optionsButton;

    //Texto de cuadro inferior derecha (tiles)
	//[SerializeField]
	//private TextMeshProUGUI tooltipText;
	//[SerializeField]
	//private Image tooltipImage;

    //Texto de cartel superior con las acciones
	[SerializeField]
	private TextMeshProUGUI tooltipAccionesText;

    //Cuadro inferior izquierda. Referencia para animaciones
	[SerializeField]
	private GameObject characterInfo;
    //Texto cuadro inferior izquierda
	[SerializeField]
	private TextMeshProUGUI characterInfoText;
    //Duración de la animación
	[SerializeField]
	private float animationDuration;
	[SerializeField]
	private float durationEndTurnRotation;
    //Posición original del cuadro para la animación.
	private Vector3 characterInfoOriginalPosition;

    //Lista que guarda los paneles en la parte superior izquierda con la info de los players
	[SerializeField]
	public List<GameObject> panelesPJ;
	//Padre de los paneles de los enemigos
	[SerializeField]
	public GameObject padrePanelesEnemigos;
	//Prefab de los paneles enemigos para instanciarlo
	[SerializeField]
	public GameObject panelesEnemigosPrefab;
	//Lista que guarda los paneles enemigos para la lista de orden de enemigos
	[HideInInspector]
	public List<GameObject> panelesEnemigos;
	
    [Header("CURSORES")]
	[SerializeField]
	public Texture2D attackCursor, movementCursor;

    //Imagen de cada player explicando sus acciones
	[SerializeField]
	public Image explanationImage;

	[SerializeField]
	public Image imageCharacterInfo;

    [Header("SCROLL")]
    //Bools que indican si se estan pulsando los botones
    private bool isScrollButtonDownBeingPressed;
    private bool isScrollButtonUpBeingPressed;

	//Bool para saber si la unidad tiene alguna unidad a rango o no
	private bool hasCharacterUnitInRange;

    //Velocidad de scroll
    [SerializeField]
    private float scrollSpeed;
    //Separación entre retratos de personajes (el 69 no va en coña, de verdad que está bien separado así)
    [SerializeField]
    private int enemyPortraitSeparation;

    //Posición inicial a la que vuelve la barra cuando se acaba el turno enemigo
    private Vector3 initialScrollPosition;

    //Topes que no puede superar la barra
    [SerializeField]
    private GameObject topScrollUp, topScrollDown, buttonUp, buttonDown;



    [Header("REFERENCIAS")]

    //Level Manager
	[HideInInspector]
    public LevelManager LM;

    #endregion

    #region INIT

    private void Awake()
    {
        LM = FindObjectOfType<LevelManager>();
    }

    //Lo pongo en una variable en vez de en el start para que lo pueda llamar el Level Manager
    public void InitializeUI()
    {
        characterInfoOriginalPosition = characterInfo.transform.position;

        //Guardo la posición inicial de la lista para poder volver a ponerla en esta posición al terminar el turno enemigo.
        initialScrollPosition = padrePanelesEnemigos.transform.position;

        for (int i = 0; i < LM.characthersOnTheBoard.Count; i++)
        {
            //Activamos los retratos necesarios y les asignamos su jugador
            panelesPJ[i].SetActive(true);
            panelesPJ[i].GetComponent<Portraits>().assignedPlayer = LM.characthersOnTheBoard[i];
            LM.characthersOnTheBoard[i].myPanelPortrait = panelesPJ[i];

            //Actualizamos las barras de vida
            panelesPJ[i].GetComponent<Portraits>().InitializeHealth();
            panelesPJ[i].GetComponent<Portraits>().RefreshTokens();
        }

        tooltipAccionesText.text = "Selecciona una unidad";
    }

    #endregion

    #region END_TURN

    //Se llama desde el botón de finalizar turno
    public void EndTurn()
    {
        //He cambiado esta parte para que el end turn también borre los tiles pintados
        if (LM.selectedCharacter == null)
        {
            if (LM.selectedEnemy != null)
            {
                LM.DeselectEnemy();
            }

            Debug.Log("Soy EndTurn");
            RotateButtonEndPhase();
            LM.ChangePhase();
        }
        else if (!LM.selectedCharacter.isMovingorRotating)
        {
            if (LM.selectedEnemy != null)
            {
                LM.DeselectEnemy();
            }

            LM.DeSelectUnit();

            Debug.Log("Soy EndTurn");
            RotateButtonEndPhase();
            LM.ChangePhase();
        }
    }

    //Función que activa o desactiva el botón de pasar turno en función de si es la fase del player o del enemigo
    public void ActivateDeActivateEndButton()
    {
        //endTurnButton.GetComponent<BoxCollider>().enabled = !endTurnButton.GetComponent<BoxCollider>().enabled;
        endTurnButton.GetComponent<MeshCollider>().enabled=true;
    }

	public void RotateButtonEndPhase()
	{
		//endTurnButton.transform.DORotate(new Vector3(-180, 0, 0), durationEndTurnRotation);
		//StartCoroutine("ButtonAnimationWaitEnd");
		endTurnButton.GetComponent<Animator>().Play("EndTurnCoinFlip");
	}

	public void RotateButtonStartPhase()
	{
		//waitingButton.transform.DORotate(new Vector3(-180, 0, 0), durationEndTurnRotation);
		//StartCoroutine("ButtonAnimationWaitStart");
		endTurnButton.GetComponent<Animator>().Play("EndTurnCoinReset");
	}

	IEnumerator ButtonAnimationWaitEnd()
	{
		yield return new WaitForSeconds(durationEndTurnRotation);
		endTurnButton.gameObject.SetActive(false);
		endTurnButton.transform.DORotate(new Vector3(0, 0, 0), durationEndTurnRotation);
		//waitingButton.gameObject.SetActive(true);
	}
	IEnumerator ButtonAnimationWaitStart()
	{
		yield return new WaitForSeconds(durationEndTurnRotation);
		//waitingButton.gameObject.SetActive(false);
		//waitingButton.transform.DORotate(new Vector3(0, 0, 0), durationEndTurnRotation);
		endTurnButton.gameObject.SetActive(true);
	}

	public void ActivateEndTurnHover()
	{
		for (int i = 0; i < LM.characthersOnTheBoard.Count; i++)
		{
			if(LM.characthersOnTheBoard[i].hasMoved == false)
			{
				LM.characthersOnTheBoard[i].actionAvaliablePanel.SetActive(true);
			}
			else if (LM.characthersOnTheBoard[i].hasAttacked == false && hasCharacterUnitInRange)
			{
				LM.characthersOnTheBoard[i].actionAvaliablePanel.SetActive(true);
			}
		}
	}

	public void DeactivateEndTurnHover()
	{
		for (int i = 0; i < LM.characthersOnTheBoard.Count; i++)
		{
			LM.characthersOnTheBoard[i].actionAvaliablePanel.SetActive(false);
		}
	}

	#endregion

	#region UNDO_MOVE

	////Se llama desde el botón de finalizar turno
	//public void UndoMove()
	//{
	//    LM.UndoMove();
	//}

	#endregion

	#region ROTATION_ARROWS

	[SerializeField]
    public void RotatePlayerInNewDirection(UnitBase.FacingDirection newDirection)
    {
        LM.selectedCharacter.RotateUnitFromButton(newDirection);
    }


	#endregion

	#region RETRATOS
	//Avisa a los retratos activos de que refresquen las barras de vida
	public void RefreshHealth()
	{
		for (int i = 0; i < LM.characthersOnTheBoard.Count; i++)
		{
            panelesPJ[i].GetComponent<Portraits>().RefreshHealth();
		}
	}
    //Avisa a los retratos activos de que refresquen los tokens
    public void RefreshTokens()
	{
        for (int i = 0; i < LM.characthersOnTheBoard.Count; i++)
        {
            panelesPJ[i].GetComponent<Portraits>().RefreshTokens();
        }
	}

	public void PortraitCharacterSelect(PlayerUnit characterToSelect)
	{
		LM.SelectUnit(characterToSelect.movementUds, characterToSelect);
	}
	public void PortraitEnemySelect(EnemyUnit enemyToSelect)
	{
		LM.selectedEnemy = enemyToSelect;
	}

	public void HighlightCharacter(PlayerUnit characterToHighlight)
	{
		characterToHighlight.SelectedColor();
	}

	public void UnHighlightCharacter(PlayerUnit characterToUnhighlight)
	{
        if (LM.selectedCharacter == null)
        {
        
            characterToUnhighlight.ResetColor();
        }
	}
    #endregion

    #region UNITS_INFO
	//Deprecated
	//public void ShowCharacterImage(UnitBase characterImage)
	//{
	//	if(characterImage.characterImage != null)
	//	{
	//		imageCharacterInfo.gameObject.SetActive(true);
	//		imageCharacterInfo.sprite = characterImage.characterImage;
	//	}
	//}

	//public void HideCharacterImage()
	//{
	//	imageCharacterInfo.gameObject.SetActive(false);
	//	imageCharacterInfo.sprite = null;
	//}

    public void ShowUnitInfo(string generalInfoText, UnitBase unitTooltipImage)
    {
        //characterInfo.transform.DOMove(characterInfo.transform.parent.position, animationDuration);
        characterInfoText.text = generalInfoText;
        if (unitTooltipImage.tooltipImage !=null)
        {
			explanationImage.gameObject.SetActive(true);
            explanationImage.sprite = unitTooltipImage.tooltipImage;
        }
    }

    public void HideUnitInfo(string textToPrint)
	{
		//characterInfo.transform.DOMove(characterInfoOriginalPosition, animationDuration);
		characterInfoText.text = textToPrint;
		explanationImage.gameObject.SetActive(false);
		explanationImage.sprite = null;
	}

	#endregion

	#region TILE_INFO

	public void ShowTileInfo(string textToPrint, Sprite tileImageToShow)
	{
		if(LM.selectedCharacter == null && LM.selectedEnemy == null)
		{
			characterInfoText.text = textToPrint;
			explanationImage.gameObject.SetActive(true);
			explanationImage.sprite = tileImageToShow;
		}	
	}

	public void HideTileInfo()
	{
		if(LM.selectedCharacter == null && LM.selectedEnemy == null)
		{
			characterInfoText.text = "";
			explanationImage.gameObject.SetActive(false);
			explanationImage.sprite = null;
		}
	}

    #endregion

    #region ENEMY_INFO

    public void SetEnemyOrder()
    {
		for (int i = 0; i < panelesEnemigos.Count; i++)
		{
			Destroy(panelesEnemigos[i].gameObject);
		}

		panelesEnemigos.Clear();

        for (int i = 0; i < LM.enemiesOnTheBoard.Count; i++)
        {
            if (!LM.enemiesOnTheBoard[i].isDead)
            {
                GameObject enemyPanel = Instantiate(panelesEnemigosPrefab, padrePanelesEnemigos.transform, false);
                enemyPanel.transform.position = new Vector3(enemyPanel.transform.position.x, enemyPanel.transform.position.y - i* enemyPortraitSeparation, enemyPanel.transform.position.z);
                panelesEnemigos.Add(enemyPanel);

                //IMPORTANTE. El contador de paneles enemigos no puede ser i ya que puede ser que haya un enemigo muerto y por tanto i sea demasiado grande.
                panelesEnemigos[panelesEnemigos.Count-1].GetComponent<EnemyPortraits>().assignedEnemy = LM.enemiesOnTheBoard[i];
                panelesEnemigos[panelesEnemigos.Count-1].GetComponent<EnemyPortraits>().enemyPortraitSprite = LM.enemiesOnTheBoard[i].characterImage;

                LM.enemiesOnTheBoard[i].GetComponent<EnemyUnit>().myPortrait = panelesEnemigos[panelesEnemigos.Count - 1].GetComponent<EnemyPortraits>();
            }
        }
    }

    public void ShowEnemyOrder(bool show_hide)
    {
        for (int i = 0; i < LM.enemiesOnTheBoard.Count; i++)
        {
            //LM.enemiesOnTheBoard[i].thisUnitOrder.SetActive(show_hide);
            LM.enemiesOnTheBoard[i].HealthBarOn_Off(show_hide);
        }
    }

    //Scrolear la barra de lista hacia arriba
    public void ScrollUp()
    {
        isScrollButtonUpBeingPressed = true;
    }

    //Scrolear la barra de lista hacia abajo
    public void ScrollDown()
    {
        isScrollButtonDownBeingPressed = true;
    }

    //Parar de scrollear la barra al soltar el botón
    public void StopScroll()
    {
        isScrollButtonDownBeingPressed = false;
        isScrollButtonUpBeingPressed = false;
    }

    //Función que hace subir a la lista cada vez que se pasa de turno enemigo
    public void ScrollUpOnce()
    {
        if (panelesEnemigos[panelesEnemigos.Count - 1].transform.position.y <= topScrollDown.transform.position.y)
        {
            padrePanelesEnemigos.transform.position = new Vector3(padrePanelesEnemigos.transform.position.x, padrePanelesEnemigos.transform.position.y + enemyPortraitSeparation, padrePanelesEnemigos.transform.position.z);
        }
    }

    //Vuelvo a poner la lista arriba del todo
    public void ResetScrollPosition()
    {
        padrePanelesEnemigos.transform.position = initialScrollPosition;
    }

    //Función que se 
    public void MoveScrollToEnemy()
    {
        //Comprobar si el retrato enemigo esta entre el tope inferior y superior
        //Comprobar si esta por encima de ambos topes o por debajo de ambos topes para decidir si subo o bajo la lista
        //Contar el número de retratos que hay hasta que haya uno dentro del cuadro para saber cuánta distancia tengo que bajar o subir la barra.
    }

    private void Update()
    {
        if (LM.currentLevelState == LevelManager.LevelState.ProcessingPlayerActions)
        {
            if (isScrollButtonDownBeingPressed)
            {
                if (panelesEnemigos[0].transform.position.y >= topScrollUp.transform.position.y)
                {
                    padrePanelesEnemigos.transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);
					buttonUp.gameObject.transform.DORotate(buttonUp.gameObject.transform.rotation.eulerAngles + new Vector3(0, 0, 10), 0.2f);
					buttonDown.gameObject.transform.DORotate(buttonDown.gameObject.transform.rotation.eulerAngles + new Vector3(0, 0, 10), 0.2f);
				}
            }

            if (isScrollButtonUpBeingPressed)
            {
                if (panelesEnemigos[panelesEnemigos.Count - 1].transform.position.y <= topScrollDown.transform.position.y)
                {
                    padrePanelesEnemigos.transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);
					buttonUp.gameObject.transform.DORotate(buttonUp.gameObject.transform.rotation.eulerAngles + new Vector3(0, 0, -10), 0.2f);
					buttonDown.gameObject.transform.DORotate(buttonDown.gameObject.transform.rotation.eulerAngles + new Vector3(0, 0, -10), 0.2f);
				}
            }
        }
    }


    #endregion

    #region TOOLTIP ACCTIONS

    public void TooltipMove()
	{
		tooltipAccionesText.text = "Mueve la unidad";
	}
	public void TooltipAttack()
	{
		tooltipAccionesText.text = "Ataca a una unidad";
		hasCharacterUnitInRange = true;
	}
	public void TooltipNoAttackable()
	{
		tooltipAccionesText.text = "Esta unidad no tiene ningún enemigo a rango";
		hasCharacterUnitInRange = false;
	}
	public void TooltipRotate()
	{
		tooltipAccionesText.text = "Selecciona la rotación de la unidad";
	}
	public void TooltipDefault()
	{
		tooltipAccionesText.text = "Selecciona una unidad";
	}
	public void TooltipMoveorAttack()
	{
		tooltipAccionesText.text = "Mueve la unidad o ataca a una unidad";
	}

	#endregion

	#region OPTIONS
	//Abre/cierra el panel de opciones y Desactiva/Activa el botón de opciones, respectivamente
	public void Activate_DeactivateOptions(bool isActivated)
	{
		optionsScreen.SetActive(isActivated);
		optionsButton.SetActive(!isActivated);
	}
	#endregion



}
