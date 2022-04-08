using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
    InventoryCellScript[] inventoryCellScripts;
    private Inventory playerInventory;
    public DropedItem dropedItem;
    public ItemDescription description;
    private InventoryCellScript _selectedCell;
    [SerializeField] private UseButton useButton;
    [SerializeField] private Slider dropItemCountSlider;
    [SerializeField] private SwordCell swordCell;
    [SerializeField] private Text money;
    public delegate void NewSelect();
    public event NewSelect onChangeSelected;

    public InventoryCellScript selectedCell
    {
        get
        {
            return _selectedCell;
        }
    }

    //��������� ��������� ������, ������� ������ ���������, ������������ ��������� �� ������ ����
    private void Start()
    {
        playerInventory = GameManager.player.GetComponent<Inventory>();
        inventoryCellScripts = gameObject.GetComponentsInChildren<InventoryCellScript>();
        gameObject.SetActive(false);
        ResetSlider();
    }
    private void OnDisable()
    {
        ClearSelected();
    }

    private void SetSlider(int max)
    {
        dropItemCountSlider.minValue = 1;
        dropItemCountSlider.interactable = true;
        dropItemCountSlider.maxValue = max;
    }
    private void ResetSlider()
    {
        dropItemCountSlider.minValue = 0;
        dropItemCountSlider.value = 0;
        dropItemCountSlider.interactable = false;
    }
    private void ChangeSelectedActive(bool isItemInCell)
    {
        if (isItemInCell)
        {
            SetSlider(System.Convert.ToInt32(_selectedCell.text.text));
        }
        else
        {
            ClearSelected();
        }
    }



    //����� ��������� ������ ���������, �� ����� � ������ ��������� ����������, ���� ��� �������
    public void ChangeSelected(InventoryCellScript newSelectedCell)
    {

        ClearSelected();
        if (newSelectedCell.item != null)
        {
            _selectedCell = newSelectedCell;
            _selectedCell.selected = true;
            _selectedCell.GetComponent<Image>().color = new Color(0.59f, 0.29f, 0.29f, 0.9f);
            SetSlider(System.Convert.ToInt32(newSelectedCell.text.text));
            useButton.ChangeActive(_selectedCell);
            onChangeSelected();
        }

    }

    //������� ������ �� ���������
    public void ClearSelected()
    {
        if (_selectedCell != null)
        {
            _selectedCell.GetComponent<Image>().color = GameManager.cellColorDefault;
            _selectedCell.selected = false;
            _selectedCell = null;
            useButton.ChangeActive(_selectedCell);
            ResetSlider();
            onChangeSelected();
        }

    }

    //������� ���� �� ���������. ���� ��������� ������ ����, �� �� ���������� �������� �� ����� ����, ����� ������� �� ���������, ������������ ���������, �������� ���������� ������
    public void DropItem()
    {
        if(_selectedCell != null)
        {
            bool isItemInCell = true;
            Vector2 side;
            if (GameManager.player.GetComponent<SpriteRenderer>().flipX)
            {
                side = new Vector2(-1f, 0);
            }
            else
            {
                side = new Vector2(1f, 0);
            }

            if (dropItemCountSlider.value >= System.Convert.ToInt32(_selectedCell.text.text))
            {
                isItemInCell = false;
            }
            for (int i = 0; i < dropItemCountSlider.value; i++)
            {
                dropedItem.DropItem(_selectedCell.item, GameManager.player.GetComponent<Rigidbody2D>().position + side);
                playerInventory.DeliteItem(_selectedCell.id);


            }
            DrawInventory();
            ChangeSelectedActive(isItemInCell);
        }
    }




    public void SetSword()
    {
        //GameObject.Find("Sword").GetComponent<Sword>().SetSword();
        if (swordCell.item!=null)
        {
            ItemScriptableObject saveSword = swordCell.item;
            swordCell.DrawCell(_selectedCell.item);
            playerInventory.ReplaceItem(_selectedCell.id, saveSword);
            
        }
        else
        {
            swordCell.DrawCell(_selectedCell.item);
            playerInventory.DeliteItem(_selectedCell.id);
            
        }
        DrawInventory();
        ChangeSelectedActive(false);
        GameManager.player.GetComponent<AttackController>().sword.SetSword(swordCell.item as SwordScriptableObject);


    }


    public void UsePotion()
    {
        GameManager.player.GetComponent<Player>().RecoverHP((_selectedCell.item as PotionScriprableObject).recoveryHP);
        string count = _selectedCell.text.text;
        playerInventory.DeliteItem(_selectedCell.id);
        DrawInventory();
        if (count == "1")
        {
            ChangeSelectedActive(false);
        }
        else
        {
            ChangeSelectedActive(true);
        }
    }

    //��������� ���������. ������� �������
    public void DrawInventory()
    {
        money.text = $"{GameManager.player.GetComponent<Player>().money}";
        ClearInventory();
        for (int i = 0; i < inventoryCellScripts.Length && i < playerInventory.inventorySlots.Count; i++)
        {
            inventoryCellScripts[i].DrawCell(playerInventory.inventorySlots[i].ItemScriptableObject, playerInventory.inventorySlots[i].count, i);
        }
    }
    //������� ��������� (����������)
    public void ClearInventory()
    {
        foreach (InventoryCellScript cell in inventoryCellScripts)
        {
            cell.OnMouseExit();
            cell.ClearCell();
            
        }
        ClearDescription();
    }
    //�������� �������� ��� ��������� �������
    public void SetDescription(AItemCell cell)
    {
        if(cell.item!=null)
        {
            ItemScriptableObject item = cell.item;
            string text = $"{item.itemName}\n\n{item.description}\n\nBase cost: {item.cost}\nSell cost: {GetSellCost(item)}";
            if (cell.item.type=="Sword")
            {
                text += $"\nDamage: {(item as SwordScriptableObject).damage}";
            }
            else if (cell.item.type == "Potion")
            {
                text += $"\nRecover: {(item as PotionScriprableObject).recoveryHP} HP";
            }
            description.SetDescription(text);
        }

    }
    //������ ��������, ����� ������ �������� ������ ���������
    public void ClearDescription()
    {
        description.ClearDescription();
    }

    public string GetSelectedCellType()
    {
        if (_selectedCell.item!=null)
        {
            return _selectedCell.item.type;
        }
        return "";
    }

    private int GetSellCost(ItemScriptableObject item)
    {
        return System.Convert.ToInt32(item.cost - (item.cost * (0.5 / GameManager.player.GetComponent<Player>().speech)));
    }
    public void SellItem()
    {
        if (_selectedCell != null)
        {
            bool isItemInCell = true;
            Player player = GameManager.player.GetComponent<Player>();
            if (dropItemCountSlider.value >= System.Convert.ToInt32(_selectedCell.text.text))
            {
                isItemInCell = false;
            }
            for (int i = 0; i < dropItemCountSlider.value; i++)
            {
                player.money += GetSellCost(_selectedCell.item);
                playerInventory.DeliteItem(_selectedCell.id);
            }
            DrawInventory();
            ChangeSelectedActive(isItemInCell);
        }
    }

}
