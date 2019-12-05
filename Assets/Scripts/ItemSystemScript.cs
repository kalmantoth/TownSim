using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum ItemType { NOTHING, ANYTHING, WOOD, STONE, BERRY, RAW_MEAT, COOKED_MEAT, RAW_FISH, COOKED_FISH, POTATO, BAKED_POTATO, WHEAT, BREAD};
public enum ItemGroup { NOT_SPECIFIED, RESOURCE, FOOD, EDIBLE_FOOD };
public enum InventoryType { EMPTY, RESOURCE, FOOD, ALL }

public static class DefaultItems
{
     public static Item wood = new Item(ItemType.WOOD, "Wood", 1, ItemGroup.RESOURCE);
     public static Item stone = new Item(ItemType.STONE, "Stone", 1, ItemGroup.RESOURCE);
     public static Item berry = new Item(ItemType.BERRY, true, true, 1, "Raspberry", 1, ItemGroup.EDIBLE_FOOD);
     public static Item rawMeat = new Item(ItemType.RAW_MEAT, true, false, 0, "Raw meat", 5, ItemGroup.FOOD);
     public static Item rawFish = new Item(ItemType.RAW_FISH, true, false, 0, "Raw fish", 5, ItemGroup.FOOD);
     public static Item cookedMeat = new Item(ItemType.COOKED_MEAT, true, true, 5, "Cooked meat", 10, ItemGroup.EDIBLE_FOOD);
     public static Item cookedFish = new Item(ItemType.COOKED_FISH, true, true, 5, "Cooked fish", 10, ItemGroup.EDIBLE_FOOD);
     public static Item potato = new Item(ItemType.POTATO, true, false, 0, "Potato", 1, ItemGroup.FOOD);
     public static Item bakedPotato = new Item(ItemType.BAKED_POTATO, true, true, 3, "Baked potato", 8, ItemGroup.EDIBLE_FOOD);
     public static Item wheat = new Item(ItemType.WHEAT, true, false, 0, "Wheat sack", 5, ItemGroup.FOOD);
     public static Item bread = new Item(ItemType.BREAD, true, true, 5, "Bread", 10, ItemGroup.EDIBLE_FOOD);
}

public class Item
{
     public ItemType itemType;
     public ItemGroup itemGroup;
     public int minQuantity;
     public int maxQuantity;
     public int currentQuantity;
     public bool isFood;
     public bool isEdible;
     public int nutritionalValue;
     public string itemName;
     public int value;

     public Item(Item item)
     {
          this.itemType = item.itemType;
          this.itemGroup = item.itemGroup;
          this.minQuantity = item.minQuantity;
          this.maxQuantity = item.maxQuantity;
          this.currentQuantity = item.currentQuantity;
          this.isFood = item.isFood;
          this.isEdible = item.isEdible;
          this.nutritionalValue = item.nutritionalValue;
          this.itemName = item.itemName;
          this.value = item.value;
     }

     public Item(ItemType itemType, string itemName, int value, ItemGroup itemGroup)
     {
          this.itemType = itemType;
          this.minQuantity = 0;
          this.maxQuantity = this.currentQuantity = 0;
          this.itemName = itemName;
          this.isFood = this.isEdible = false;
          this.value = value;
          this.itemGroup = itemGroup;
     }

     public Item(ItemType itemType, bool isFood, bool isEdible, int nutritionalValue, string itemName, int value, ItemGroup itemGroup)
     {
          this.itemType = itemType;
          this.minQuantity = 0;
          this.maxQuantity = this.currentQuantity = 0;
          this.itemName = itemName;
          this.isFood = isFood;
          this.isEdible = isEdible;
          this.nutritionalValue = nutritionalValue;
          this.value = value;
          this.itemGroup = itemGroup;
     }
     
     public Item(ItemType itemType)
     {
          this.itemType = itemType;
          this.minQuantity = 0;
          this.maxQuantity = 100;
          this.currentQuantity = 0;
          
          this.itemName = "";
          this.value = 0;
          ItemEdibleCheckInConstructor(itemType);
     }

     public Item(ItemType itemType, int maxQuantity, string itemName = "")
     {
          this.itemType = itemType;
          this.minQuantity = this.currentQuantity = 0;
          this.maxQuantity = maxQuantity;
          
          this.itemName = itemName;
          this.value = 0;
          ItemEdibleCheckInConstructor(itemType);

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
          this.value = 0;
          ItemEdibleCheckInConstructor(itemType);

     }

     private void ItemEdibleCheckInConstructor(ItemType itemType)
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
     
     
     public override string ToString()
     {
          return Utils.UppercaseFirst(itemType.ToString()) + " (" + currentQuantity + "/" + maxQuantity + ")";
     }
     
     public string ToStringAllStat()
     {
          return itemType + " " + itemGroup + " " + minQuantity + " " + maxQuantity + " " + currentQuantity + " " + isFood + " " + isEdible + " " + nutritionalValue + " " + itemName + " " + value;
     }

     public override bool Equals(object obj)
     {
          return obj is Item item &&
                 itemType == item.itemType &&
                 itemGroup == item.itemGroup &&
                 minQuantity == item.minQuantity &&
                 maxQuantity == item.maxQuantity &&
                 currentQuantity == item.currentQuantity &&
                 isFood == item.isFood &&
                 isEdible == item.isEdible &&
                 nutritionalValue == item.nutritionalValue &&
                 itemName.Equals(item.itemName) &&
                 value == item.value;
     }

     // Need to implement because of the equals method
     public override int GetHashCode()
     {
          return base.GetHashCode();
     }
}



public class Inventory
{
     private string[] itemTypeEnumNames = Enum.GetNames(typeof(ItemType));
     private ItemType[] itemTypeEnumValues = (ItemType[])Enum.GetValues(typeof(ItemType));

     public InventoryType inventoryType;
     public Item[] items = new Item[Enum.GetValues(typeof(ItemType)).Length];
     

     

     public Inventory(int maxItemQuantity = 100, InventoryType inventoryType = InventoryType.ALL)
     {
          this.inventoryType = inventoryType;
          
          if(inventoryType == InventoryType.FOOD)
          {
               for (int i = 0; i < itemTypeEnumValues.Length; i++)
               {
                    foreach (System.Reflection.FieldInfo field in typeof(DefaultItems).GetFields())
                    {
                         Item defItem = (Item)field.GetValue(null);
                         if(defItem.itemType == itemTypeEnumValues[i] && (defItem.itemGroup == ItemGroup.FOOD || defItem.itemGroup == ItemGroup.EDIBLE_FOOD))
                         {
                              items[i] = new Item(defItem.itemType, maxItemQuantity, defItem.itemName);
                              Debug.Log("Granary is initalized with item: " + items[i].itemName + " quantity: " + items[i].currentQuantity + " / " + items[i].maxQuantity);
                         }
                         else if (items[i] == null) items[i] = new Item(itemTypeEnumValues[i], 0, 0);
                    }
               }
          }
          else if(inventoryType == InventoryType.RESOURCE)
          {
               for (int i = 0; i < itemTypeEnumValues.Length; i++)
               {
                    foreach (System.Reflection.FieldInfo field in typeof(DefaultItems).GetFields())
                    {
                         Item defItem = (Item)field.GetValue(null);
                         if (defItem.itemType == itemTypeEnumValues[i] && defItem.itemGroup == ItemGroup.RESOURCE)
                         {
                              items[i] = new Item(defItem.itemType, maxItemQuantity, defItem.itemName);
                              Debug.Log("Storage is initalized with item: " + items[i].itemName + " quantity: " + items[i].currentQuantity + " / " + items[i].maxQuantity);
                         }
                         else if(items[i] == null) items[i] = new Item(itemTypeEnumValues[i], 0, 0);
                    }
               }

               foreach (Item i in items)
               {
                    Debug.Log(i.ToString());
               }
          }
          else if(inventoryType == InventoryType.ALL)
          {
               for (int i = 0; i < itemTypeEnumValues.Length; i++)
               {
                    items[i] = new Item(itemTypeEnumValues[i], maxItemQuantity);
               }
          }


     }

     public List<ItemType> FindNonEmptyEdibleItemTypes()
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
     
     public void TransferFullItemStackToInventory(Inventory otherInventory) // Transfer full items from the main inventory to the other     
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
    

     public override string ToString()
     {

          
          string returnString = "";
          foreach (Item item in items)
          {
               if (item.currentQuantity != 0)
               {
                    returnString += item.ToString() + "   ";
               }
          }

          if(returnString.Equals("")) returnString = "Empty";

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
