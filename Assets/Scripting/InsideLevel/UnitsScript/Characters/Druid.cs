﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Druid : PlayerUnit
{
    #region VARIABLES

   
    [Header("MEJORAS DE PERSONAJE")]

    [SerializeField]
    public  int healedLife;

    [Header("Activas")]
    //ACTIVAS

    //La activa uno depende de cambiar el int healedLife;

    //bool mejora de la activa 1
    public bool individualHealer2;

    //int que indica cuantas unidades de movimiento se mejora a la unidad
    //Jojo, acuerdate de que hay que incremeentar healedLife más aún
    public int movementUpgrade;


    //bool activa 2
    public bool areaHealer;

    //bool mejora activa 2
    public bool areaHealer2;

    [Header("Pasivas")]
    //PASIVAS

    //bool pasiva 1
    public bool tileTransformer;

    //bool pasiva 2
    public bool tileSustitute;

    //bool mejora de la pasiva 2
    public bool tileSustitute2;

    //int que añade bonus al druida si está en un tile de curación
    public int bonusOnTile;

    public GameObject healerTilePref;

    #endregion


    //En función de donde este mirando el personaje paso una lista de tiles diferente.
    public override void Attack(UnitBase unitToAttack)
    {
        hasAttacked = true;

        if (unitToAttack.isMarked)
        {
            unitToAttack.isMarked = false;
            currentHealth += FindObjectOfType<Monk>().healerBonus;

            if (FindObjectOfType<Monk>().debuffMark2)
            {
                if (!unitToAttack.isStunned)
                {
                    unitToAttack.isStunned = true;
                    unitToAttack.turnStunned = 1;
                }

            }
            else if (FindObjectOfType<Monk>().healerMark2)
            {
                BuffbonusStateDamage = 1;

            }

            UIM.RefreshTokens();

        }

        if (areaHealer)
        {
            //Hay que cambiar
            Instantiate(attackParticle, unitToAttack.transform.position, unitToAttack.transform.rotation);

            currentHealth -= 1;
            UIM.RefreshTokens();
            UIM.RefreshHealth();

            //COMPROBAR QUE NO DE ERROR EN OTRAS COSAS
            TM.surroundingTiles.Clear();

            TM.GetSurroundingTiles(unitToAttack.myCurrentTile, 1, true, false);
            //Hago daño a las unidades adyacentes
            for (int i = 0; i < TM.surroundingTiles.Count; ++i)
            {
                if (TM.surroundingTiles[i].unitOnTile != null)
                {
                    if (areaHealer2)
                    {
                        TM.surroundingTiles[i].unitOnTile.isStunned = false;
                        TM.surroundingTiles[i].unitOnTile.turnStunned = 0;
                        TM.surroundingTiles[i].unitOnTile.hasFear = false;
                        TM.surroundingTiles[i].unitOnTile.turnsWithFear = 0;
                        TM.surroundingTiles[i].unitOnTile.BuffbonusStateDamage = 0;
                        
                    }
                    if (tileTransformer)
                    {
                        Instantiate(healerTilePref, TM.surroundingTiles[i].unitOnTile.transform.position, TM.surroundingTiles[i].unitOnTile.transform.rotation);

                    }
                    TM.surroundingTiles[i].unitOnTile.currentHealth += healedLife;
                }
            }
          
        }
        else
        {
            //Hay que cambiar
            Instantiate(attackParticle, unitToAttack.transform.position, unitToAttack.transform.rotation);

            if (unitToAttack.GetComponent<PlayerUnit>())
            {
                //Hay que cambiar
                SoundManager.Instance.PlaySound(AppSounds.MAGE_ATTACK);
                if (individualHealer2)
                {
                    unitToAttack.movementUds = unitToAttack.GetComponent<PlayerUnit>().fMovementUds + movementUpgrade;
                }
                else if (tileTransformer)
                {
                    Instantiate(healerTilePref, unitToAttack.transform.position, unitToAttack.transform.rotation);

                }
                unitToAttack.currentHealth += healedLife;
                currentHealth -= 1;
                UIM.RefreshTokens();
                UIM.RefreshHealth();
            }
            else
            {
                //Hago daño
                DoDamage(unitToAttack);

                if (currentHealth < maxHealth)
                {
                    currentHealth += healedLife;
                    UIM.RefreshTokens();
                    UIM.RefreshHealth();
                }

                //Hay que cambiar
                SoundManager.Instance.PlaySound(AppSounds.MAGE_ATTACK);

            }
        }
        

        //La base tiene que ir al final para que el bool de hasAttacked se active después del efecto.
        base.Attack(unitToAttack);
        
    }

    protected override void DoDamage(UnitBase unitToDealDamage)
    {

        //Añado este if para el count de honor del samurai
        if (currentFacingDirection == FacingDirection.North && unitToDealDamage.currentFacingDirection == FacingDirection.South
       || currentFacingDirection == FacingDirection.South && unitToDealDamage.currentFacingDirection == FacingDirection.North
       || currentFacingDirection == FacingDirection.East && unitToDealDamage.currentFacingDirection == FacingDirection.West
       || currentFacingDirection == FacingDirection.West && unitToDealDamage.currentFacingDirection == FacingDirection.East
       )
        {
            LM.honorCount++;
        }

        base.DoDamage(unitToDealDamage);
    }

    #region CHECKS
    //AL igual que con el Mago, se hace override a esta función para que pueda atravesar unidades al atacar.
    public override void CheckUnitsAndTilesInRangeToAttack()
    {
        currentUnitsAvailableToAttack.Clear();
        previousTileHeight = 0;

        if (currentFacingDirection == FacingDirection.North)
        {
            if (attackRange <= myCurrentTile.tilesInLineUp.Count)
            {
                rangeVSTilesInLineLimitant = attackRange;
            }
            else
            {
                rangeVSTilesInLineLimitant = myCurrentTile.tilesInLineUp.Count;
            }

            for (int i = 0; i < rangeVSTilesInLineLimitant; i++)
            {
                //Guardo la altura mas alta en esta linea de tiles
                if (myCurrentTile.tilesInLineUp[i].height > previousTileHeight)
                {
                    previousTileHeight = myCurrentTile.tilesInLineUp[i].height;
                }

                //Si hay una unidad
                if (myCurrentTile.tilesInLineUp[i].unitOnTile != null)
                {
                    //Compruebo que la diferencia de altura con mi tile y con el tile anterior es correcto.
                    if (Mathf.Abs(myCurrentTile.tilesInLineUp[i].height - myCurrentTile.height) <= maxHeightDifferenceToAttack
                        || Mathf.Abs(myCurrentTile.tilesInLineUp[i].height - previousTileHeight) <= maxHeightDifferenceToAttack)
                    {

                        if (myCurrentTile.tilesInLineUp[i].unitOnTile.currentHealth == myCurrentTile.tilesInLineUp[i].unitOnTile.maxHealth
                            && myCurrentTile.tilesInLineUp[i].unitOnTile.GetComponent<PlayerUnit>())
                        {

                        }
                        else
                        {
                            //Almaceno la primera unidad en la lista de posibles unidades
                            currentUnitsAvailableToAttack.Add(myCurrentTile.tilesInLineUp[i].unitOnTile);
                        }
                        
                    }

                    else
                    {
                        continue;
                    }
                }

                if (myCurrentTile.tilesInLineUp[i].isEmpty)
                {
                    break;
                }
            }
        }

        if (currentFacingDirection == FacingDirection.South)
        {
            if (attackRange <= myCurrentTile.tilesInLineDown.Count)
            {
                rangeVSTilesInLineLimitant = attackRange;
            }
            else
            {
                rangeVSTilesInLineLimitant = myCurrentTile.tilesInLineDown.Count;
            }

            for (int i = 0; i < rangeVSTilesInLineLimitant; i++)
            {
                //Guardo la altura mas alta en esta linea de tiles
                if (myCurrentTile.tilesInLineDown[i].height > previousTileHeight)
                {
                    previousTileHeight = myCurrentTile.tilesInLineDown[i].height;
                }

                //Si hay una unidad
                if (myCurrentTile.tilesInLineDown[i].unitOnTile != null)
                {
                    //Compruebo que la diferencia de altura con mi tile y con el tile anterior es correcto.
                    if (Mathf.Abs(myCurrentTile.tilesInLineDown[i].height - myCurrentTile.height) <= maxHeightDifferenceToAttack
                        || Mathf.Abs(myCurrentTile.tilesInLineDown[i].height - previousTileHeight) <= maxHeightDifferenceToAttack)
                    {
                        if (myCurrentTile.tilesInLineDown[i].unitOnTile.currentHealth == myCurrentTile.tilesInLineDown[i].unitOnTile.maxHealth
                           && myCurrentTile.tilesInLineDown[i].unitOnTile.GetComponent<PlayerUnit>())
                        {

                        }
                        else
                        {
                            //Almaceno la primera unidad en la lista de posibles unidades
                            currentUnitsAvailableToAttack.Add(myCurrentTile.tilesInLineDown[i].unitOnTile);
                        }
                        
                    }

                    else
                    {
                        continue;
                    }
                }

                if (myCurrentTile.tilesInLineDown[i].isEmpty)
                {
                    break;
                }
            }
        }

        if (currentFacingDirection == FacingDirection.East)
        {
            if (attackRange <= myCurrentTile.tilesInLineRight.Count)
            {
                rangeVSTilesInLineLimitant = attackRange;
            }
            else
            {
                rangeVSTilesInLineLimitant = myCurrentTile.tilesInLineRight.Count;
            }

            for (int i = 0; i < rangeVSTilesInLineLimitant; i++)
            {
                //Guardo la altura mas alta en esta linea de tiles
                if (myCurrentTile.tilesInLineRight[i].height > previousTileHeight)
                {
                    previousTileHeight = myCurrentTile.tilesInLineRight[i].height;
                }

                //Si hay una unidad
                if (myCurrentTile.tilesInLineRight[i].unitOnTile != null)
                {
                    //Compruebo que la diferencia de altura con mi tile y con el tile anterior es correcto.
                    if (Mathf.Abs(myCurrentTile.tilesInLineRight[i].height - myCurrentTile.height) <= maxHeightDifferenceToAttack
                        || Mathf.Abs(myCurrentTile.tilesInLineRight[i].height - previousTileHeight) <= maxHeightDifferenceToAttack)
                    {

                        if (myCurrentTile.tilesInLineRight[i].unitOnTile.currentHealth == myCurrentTile.tilesInLineRight[i].unitOnTile.maxHealth
                           && myCurrentTile.tilesInLineRight[i].unitOnTile.GetComponent<PlayerUnit>())
                        {

                        }
                        else
                        {
                            //Almaceno la primera unidad en la lista de posibles unidades
                            currentUnitsAvailableToAttack.Add(myCurrentTile.tilesInLineRight[i].unitOnTile);
                        }
                       
                    }

                    else
                    {
                        continue;
                    }
                }

                if (myCurrentTile.tilesInLineRight[i].isEmpty)
                {
                    break;
                }
            }
        }

        if (currentFacingDirection == FacingDirection.West)
        {
            if (attackRange <= myCurrentTile.tilesInLineLeft.Count)
            {
                rangeVSTilesInLineLimitant = attackRange;
            }
            else
            {
                rangeVSTilesInLineLimitant = myCurrentTile.tilesInLineLeft.Count;
            }

            for (int i = 0; i < rangeVSTilesInLineLimitant; i++)
            {
                //Guardo la altura mas alta en esta linea de tiles
                if (myCurrentTile.tilesInLineLeft[i].height > previousTileHeight)
                {
                    previousTileHeight = myCurrentTile.tilesInLineLeft[i].height;
                }

                //Si hay una unidad
                if (myCurrentTile.tilesInLineLeft[i].unitOnTile != null)
                {
                    //Compruebo que la diferencia de altura con mi tile y con el tile anterior es correcto.
                    if (Mathf.Abs(myCurrentTile.tilesInLineLeft[i].height - myCurrentTile.height) <= maxHeightDifferenceToAttack
                        || Mathf.Abs(myCurrentTile.tilesInLineLeft[i].height - previousTileHeight) <= maxHeightDifferenceToAttack)
                    {


                        if (myCurrentTile.tilesInLineLeft[i].unitOnTile.currentHealth == myCurrentTile.tilesInLineLeft[i].unitOnTile.maxHealth
                           && myCurrentTile.tilesInLineLeft[i].unitOnTile.GetComponent<PlayerUnit>())
                        {

                        }
                        else
                        {
                            //Almaceno la primera unidad en la lista de posibles unidades
                            currentUnitsAvailableToAttack.Add(myCurrentTile.tilesInLineLeft[i].unitOnTile);
                        }
                       
                    }

                    else
                    {
                        continue;
                    }
                }

                if (myCurrentTile.tilesInLineLeft[i].isEmpty)
                {
                    break;
                }
            }

        }

        //Marco las unidades disponibles para atacar de color rojo
        for (int i = 0; i < currentUnitsAvailableToAttack.Count; i++)
        {
            CalculateDamage(currentUnitsAvailableToAttack[i]);
            currentUnitsAvailableToAttack[i].ColorAvailableToBeAttacked(damageWithMultipliersApplied);
            
            currentUnitsAvailableToAttack[i].HealthBarOn_Off(true);
        }
    }
    #endregion
}
