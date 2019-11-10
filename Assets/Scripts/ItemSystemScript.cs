using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum ItemType { NOTHING, WOOD, STONE, GOLD, BERRY, RAW_MEAT, COOKED_MEAT, RAW_FISH, COOKED_FISH, POTATO, BAKED_POTATO, WHEAT, BREAD, ANYTHING};
public enum ItemGroup { NOTHING, RESOURCE, FOOD, EDIBLE_FOOD, ALL };



public class Item
{
     public ItemType itemType;
     public int minQuantity;
     public int maxQuantity;
     public int currentQuantity;
     public string itemName;
     public bool isFood;
     public bool isEdible;

     public Item(ItemType itemType)
     {
          this.itemType = itemType;
          this.minQuantity = 0;
          this.maxQuantity = 100;
          this.currentQuantity = 0;

          this.itemName = "";
          itemEdibleCheckInConstructor(itemType);
     }

     public Item(ItemType itemType, int maxQuantity)
     {
          this.itemType = itemType;
          this.minQuantity = this.currentQuantity = 0;
          this.maxQuantity = maxQuantity;
          
          this.itemName = "";
          itemEdibleCheckInConstructor(itemType);

     }

     public Item(ItemType itemType, int currentQuantity, int maxQuantity)
     {
          this.itemType = itemType;
          this.minQuantity = 0;
          this.maxQuantity = maxQuantity;

          if (currentQuantity > maxQuantity) this.currentQuantity = maxQuantity;
          else if (currentQuantity < minQuantity) this.currentQuantity = minQuantity;
          else this.currentQuantity = currentQuantity;
          
          this.itemName = "";
          itemEdibleCheckInConstructor(itemType);

     }

     private void itemEdibleCheckInConstructor(ItemType itemType)
     {
          switch (itemType)
          {
               case ItemType.RAW_FISH:
               case ItemType.RAW_MEAT:
               case ItemType.POTATO:
               case ItemType.WHEAT:
                    this.isFood = true;
                    this.isEdible = false;
                    break;
               case ItemType.BAKED_POTATO:
               case ItemType.BREAD:
               case ItemType.BERRY:
               case ItemType.COOKED_FISH:
               case ItemType.COOKED_MEAT:
                    this.isFood = true;
                    this.isEdible = true;
                    break;
               default:
                    this.isFood = false;
                    this.isEdible = false;
                    break;
          }
     }

     public bool CanModifyItemQuantity(int modifyingValue)
     {
          if (currentQuantity + modifyingValue >= minQuantity && currentQuantity + modifyingValue <= maxQuantity)
          {
               return true;
          }
          return false;
     }

     public bool ModifyItemQuantity(int modifyingValue)
     {
          if (currentQuantity + modifyingValue >= minQuantity && currentQuantity + modifyingValue <= maxQuantity)
          {
               currentQuantity += modifyingValue;
               return true;
          }
          return false;
     }
     
     public string ToString()
     {
          return itemType.ToString() + " (" + currentQuantity + "/" + maxQuantity + ")";
     }
}



public class Inventory
{
     string[] itemTypeEnumNames = Enum.GetNames(typeof(ItemType));
     ItemType[] itemTypeEnumValues = (ItemType[])Enum.GetValues(typeof(ItemType));

     public ItemGroup itemGroup;
     public Item[] items = new Item[Enum.GetValues(typeof(ItemType)).Length];

     

     public Inventory(int maxItemQuantity = 100, ItemGroup itemGroup = ItemGroup.ALL)
     {
          this.itemGroup = itemGroup;


          for(int i = 0; i < itemTypeEnumValues.Length; i++)
          {
               items[i] = new Item(itemTypeEnumValues[i], maxItemQuantity);
          }
          
     }

     public List<ItemType> findNonEmptyEdibleItemTypes()
     {
          List<ItemType> nonEmptyEdibleItemTypes = new List<ItemType>();

          foreach (Item item in items)
          {
               if (item.isEdible && item.minQuantity != item.currentQuantity) nonEmptyEdibleItemTypes.Add(item.itemType);
          }

          if (nonEmptyEdibleItemTypes.Count != 0) return nonEmptyEdibleItemTypes;
          else return null;
     }

     public bool CanModifyItemQuantity(ItemType itemType,int modifyingValue)
     {
          return items[(int)itemType].CanModifyItemQuantity(modifyingValue);
     }

     public bool ModifyInventory(ItemType itemType, int modifyingValue)
     {
          return items[(int)itemType].ModifyItemQuantity(modifyingValue);
     }
     

     public int GetItemQuantity(ItemType itemType)
     {
          return items[(int)itemType].currentQuantity;
     }

     public int GetMaxItemQuantity(ItemType itemType)
     {
          return items[(int)itemType].maxQuantity;
     }

     public int GetItemQuantityToReachMax(ItemType itemType)
     {
          return items[(int)itemType].maxQuantity - items[(int)itemType].currentQuantity;
     }

     public int GetItemQuantityToReachMin(ItemType itemType)
     {
          return items[(int)itemType].maxQuantity - items[(int)itemType].currentQuantity;
     }


     public bool IsItemTypeFull(ItemType itemType)
     {
          if (items[(int)itemType].currentQuantity == items[(int)itemType].maxQuantity) return true;
          else return false;
     }

     public bool IsThereFullItemStack()
     {
          foreach (Item item in items)
          {
               if (item.currentQuantity == item.maxQuantity)
               {
                    return true;
               }
          }
          return false;
     }

     public bool IsThereFoodWhich(bool isEdible)
     {
          foreach (Item item in items)
          {
               if (item.currentQuantity != 0 && item.isEdible == isEdible && item.isFood)
               {
                    return true;
               }
          }
          return false;
     }

     public ItemType GetFoodItemTypeWhich(bool isEdible)
     {
          foreach (Item item in items)
          {
               if (item.currentQuantity != 0 && item.isEdible == isEdible && item.isFood)
               {
                    return item.itemType;
               }
          }
          return ItemType.NOTHING;
     }

     public ItemType GetFirstFoodWhich(bool isEdible)
     {
          foreach (Item item in items)
          {
               if (item.currentQuantity != 0 && item.isEdible == isEdible && item.isFood)
               {
                    return item.itemType;
               }
          }

          return ItemType.NOTHING;
     }



     public ItemType FullItemStackItemType()
     {
          foreach (Item item in items)
          {
               if (item.currentQuantity == item.maxQuantity)
               {
                    return item.itemType;
               }
          }
          return ItemType.NOTHING;
          
     }
     
    
     public void TransferFullItemStackToInventory(Inventory otherInventory, ItemGroup otherInventoryType) // Transfer full items from the main inventory to the other     
     {
          for (int i = 0; i < itemTypeEnumValues.Length; i++) 
          {
               if (otherInventory.items[i].ModifyItemQuantity(items[i].currentQuantity))   
               {
                    items[i].currentQuantity = 0;
               }
          }
     }
     
     public int GetItemCurrentQuantity(ItemType itemType)
     {
          return items[(int)itemType].currentQuantity;
     }
    

     public string ToString()
     {
          string returnString = "";
          foreach (Item item in items)
          {
               if (item.currentQuantity != 0)
               {
                    returnString += item.ToString() + "   ";
               }
          }
          return returnString;
     }
     

}



public class ItemSystemScript : MonoBehaviour
{
    void Start()
    {
        
    }
     
    void Update()
    {
        
    }
}
