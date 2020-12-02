﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayFieldLogic : MonoBehaviour
{
    
    private void Awake()
    {
        // Создаем текущего игрока.
        Player.Init();
    }
    private void Start()
    {
        Player.SetTestTownAndUnitKit();
    }

    /// <summary>
    /// Перемещает камеру к точке.
    /// </summary>
    /// <param name="pointCoordinates">Координаты точки, к которой перемещается камера.</param>
    private void MoveCameraToPoint(Vector3Int pointCoordinates)
    {
        Vector3 cameraMovement = pointCoordinates - Camera.main.transform.position;
        // После вычисления перемещения информация о координате z теряется
        // Координата z должна быть равна -10, чтобы камера могла видеть все объекты
        cameraMovement.z = -10;
        Camera.main.transform.Translate(cameraMovement);
    }

    /// <summary>
    /// Проверяем, кликнул ли пользователь по своему городу. Если кликнул, обрабатываем нажатие.
    /// </summary>
    /// <param name="coordinates">Координаты ячейки, в которую пользователь кликнул ЛКМ.</param>
    private void ProcessClickOnPlayerCity(Vector3Int coordinates)
    {
        // Пробегаемся по списку городов и проверяем наличие нажатия пользователем по одному из городов.
        foreach (City city in Player.listOfCities)
        {
            // Если пользователь нажал на какой-либо из своих городов
            if (city.Coordinates == coordinates)
            {
                // Отмечаем данный город как выбранный
                Player.SelectCity(city);
                // Перемещаем к выбранному городу камеру.
                MoveCameraToPoint(coordinates);
                
            }
        }
    }


    /// <summary>
    /// Проверяем, кликнул ли пользователь по своему юниту. Если кликнул, обрабатываем нажатие.
    /// </summary>
    /// <param name="coordinates"></param>
    private void ProcessClickOnPlayerUnit(Vector3Int coordinates)
    {
        foreach (Human human in Player.listOfUnits)
        {
            // Если пользователь щелкнул ЛКМ по юниту, то он его выбирает. 
            if (human.Coordinates == coordinates)
            {
                Player.SelectUnit(human);
            }
        }
    }

    /// <summary>
    /// Проверяет, существует ли по данным координатам клетка игрового поля
    /// </summary>
    /// <param name="tileCoordinates">Координаты клетки для проверки</param>
    /// <returns>Если клетка существует, true. Иначе - false</returns>
    public static bool IsTileExists(Vector3Int tileCoordinates)
    {
        if (GameData.baseLayer.GetTile(tileCoordinates) != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Считает количество очков действия для того, чтобы переместить юнит в данную ячейку.
    /// </summary>
    /// <param name="coordinates">Координаты ячейки</param>
    /// <returns>Количество очков действия, которые требуются для перемещения в данную ячейку</returns>
    public static int GetTileAPRequirments(Vector3Int coordinates)
    {
        Tile tile = (Tile)GameData.baseLayer.GetTile(coordinates);
        if (tile == null)
        {
            return -1;
        }
        // Проверяем, не требует ли данная ячейка 1 ОД
        foreach (string item1AP in GameData.tilesRequired1AP)
        {
            if(item1AP == tile.name)
            {
                return 1;
            }
        }
        foreach(string item2AP in GameData.tilesRequired2AP)
        {
            if(item2AP == tile.name)
            {
                return 2;
            }
        }
        
        return 3;
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            MenuPanelLogic.IsMenuPanelActive = true;
            MenuPanelLogic.menuBackground.SetActive(true);
        }
        // Все действия игроком на игровом поле обрабатываются, если внутриигровое меню неактивно.
        if (!MenuPanelLogic.IsMenuActive())
        {
            StatusBarLogic.UpdateStatusBar();
            // Обрабатываем нажатия пользователем левой кнопкой мыши по городам, юнитам и ландшафту
            // С помощью левой кнопки мыши пользователь может выбрать юнита для передвижения
            if (Input.GetMouseButtonDown(0))
            {
                // Получаем координаты точки в пространстве, в которую пользователь кликнул
                Vector3 clickedWorldCoordinates = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // Конвертируем позицию этой точки в координаты ячейки, в которой она содержится
                Vector3Int clickedTileCoordinates = GameData.baseLayer.WorldToCell(clickedWorldCoordinates);
                // Проверяем, кликнул ли пользователь ЛКМ по одному из своих городов:
                ProcessClickOnPlayerCity(clickedTileCoordinates);
                // Проверяем, кликнул ли пользователь ЛКМ по одному из своих юнитов:
                ProcessClickOnPlayerUnit(clickedTileCoordinates);
            }

            // С помощью правой кнопки мыши юнитов можно будет передвигать
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 clickedWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int destination = GameData.baseLayer.WorldToCell(clickedWorldPosition);

                if (Player.selectedUnit != null && Player.selectedUnit.IsMovingPossible(destination))
                {
                    // Если юнит выбран и движение в выбранную клетку возможно, производим перемещение юнита
                    Player.selectedUnit.Move(destination);     // Производим перемещение юнита в указанную клетку
                    UnitInfoPanelLogic.UpdateUnitInfo(Player.selectedUnit);   // Обновляем информацию о юните в UnitInfoPanel
                    Player.selectedUnit.HideTilesForMoving();  // После движения убираем сетку возможных тайлов для передвижения
                    Player.selectedUnit.SetTilesForMoving();   // Просчитываем новую сетку возможных передвижений
                    if (Player.selectedUnit.ActionPoints > 0)
                    {
                        Player.selectedUnit.ShowTilesForMoving();  // Вновь отрисовываем сетку возможных тайлов для перемещения
                    }
                }
            }
        }
    }
}
