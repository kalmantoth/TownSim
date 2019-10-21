using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum ResourceType { NOTHING, WOOD, WOOD_PROCESSED, STONE, STONE_PROCESSED, GOLD, FOOD };
public enum FoodType { NOTHING, BERRY, RAW_MEAT, COOKED_MEAT, RAW_FISH, COOKED_FISH };
public enum ItemType { NOTHING, RESOURCE, FOOD };
public enum InventoryType { RESOURCE, FOOD, ALL };

public class Item
{
     public int value;

     public Item()
     {
          this.value = 0;
     }

     public Item(int value)
     {
          this.value = value;
     }

     public string ToString()
     {
          return value.ToString();
     }
}

public class ResourceItem : Item
{
     public ResourceType resourceType;

     public ResourceItem(ResourceType resourceType) : base()
     {
          this.resourceType = resourceType;
     }

     public ResourceItem(ResourceType resourceType, int value) : base(value)
     {
          this.resourceType = resourceType;
     }


     public string ToString()
     {
          if (this.value > 0) return "Resource / " + resourceType.ToString() + " (" + value + " gold)";
          else return "Resource / " + resourceType.ToString();
     }
}

public class FoodItem : Item
{
     public FoodType foodType;

     public FoodItem(FoodType foodType) : base()
     {
          this.foodType = foodType;
     }

     public FoodItem(FoodType foodType, int value) : base(value)
     {
          this.foodType = foodType;
     }


     public string ToString()
     {
          if (this.value > 0) return "Food / " + foodType.ToString() + " (" + value + " gold)";
          else return "Food / " + foodType.ToString();
     }
}

public class ItemStack
{

     public Item item;
     public int minQuantity;
     public int maxQuantity;
     public int currentQuantity;

     public ItemStack(Item item)
     {
          this.item = item;
          this.minQuantity = 0;
          this.maxQuantity = 100;
          this.currentQuantity = 0;
     }

     public ItemStack(Item item, int minQuantity, int maxQuantity, int currentQuantity)
     {
          this.item = item;
          this.minQuantity = minQuantity;
          this.maxQuantity = maxQuantity;

          if (currentQuantity > maxQuantity)
          {
               this.currentQuantity = this.maxQuantity;
          }
          else this.currentQuantity = currentQuantity;
     }

     public bool ModifyItemStack(int modifyingValue)
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
          if (item is ResourceItem)
          {
               return ((ResourceItem)item).ToString() + " " + this.currentQuantity + "/" + this.maxQuantity;
          }
          else if (item is FoodItem)
          {
               return ((FoodItem)item).ToString() + " " + this.currentQuantity + "/" + this.maxQuantity;
          }
          else
          {
               return item.ToString() + " " + this.currentQuantity + "/" + this.maxQuantity;
          }


     }




}

public class Inventory
{
     public InventoryType inventoryType;
     public int maxItemQuantity;
     ItemStack[] itemStacks;

     int resourceTypeEnumLength = Enum.GetValues(typeof(ResourceType)).Length;
     int foodTypeEnumLength = Enum.GetValues(typeof(FoodType)).Length;

     public Inventory(int maxItemQuantity = 100, InventoryType inventoryType = InventoryType.ALL)
     {
          this.inventoryType = inventoryType;
          this.maxItemQuantity = maxItemQuantity;

          if (inventoryType == InventoryType.RESOURCE)           // Inventory that can hold only resources
          {
               itemStacks = new ItemStack[resourceTypeEnumLength];
               int index = 0;
               foreach (ResourceType resType in Enum.GetValues(typeof(ResourceType)))
               {
                    itemStacks[index] = new ItemStack(new ResourceItem(resType), 0, maxItemQuantity, 0);
                    index++;
               }
          }
          else if (inventoryType == InventoryType.FOOD)          // Inventory that can hold only foods
          {
               itemStacks = new ItemStack[foodTypeEnumLength];
               int index = 0;
               foreach (FoodType foodType in Enum.GetValues(typeof(FoodType)))
               {
                    itemStacks[index] = new ItemStack(new FoodItem(foodType), 0, maxItemQuantity, 0);
                    index++;
               }
          }
          else if (inventoryType == InventoryType.ALL)           // Inventory that can hold resources and foods too
          {
               // Add all the ItemStack to the inventory at the initalization
               itemStacks = new ItemStack[resourceTypeEnumLength + foodTypeEnumLength];
               int index = 0;
               foreach (ResourceType resType in Enum.GetValues(typeof(ResourceType)))
               {
                    itemStacks[index] = new ItemStack(new ResourceItem(resType), 0, maxItemQuantity, 0);
                    index++;
               }

               foreach (FoodType foodType in Enum.GetValues(typeof(FoodType)))
               {
                    itemStacks[index] = new ItemStack(new FoodItem(foodType), 0, maxItemQuantity, 0);
                    index++;
               }
          }

     }

     public bool ModifyInventory(ResourceType resourceType, int modifyingValue)
     {
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.item is ResourceItem)
               {
                    ResourceItem resourceItem = (ResourceItem)iStack.item;

                    if (resourceItem.resourceType == resourceType)
                    {
                         return iStack.ModifyItemStack(modifyingValue);
                    }
               }
          }
          return false;
     }

     public bool ModifyInventory(FoodType foodType, int modifyingValue)
     {
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.item is FoodItem)
               {
                    FoodItem foodItem = (FoodItem)iStack.item;

                    if (foodItem.foodType == foodType)
                    {
                         return iStack.ModifyItemStack(modifyingValue);
                    }

               }
          }
          return false;
     }

     public int GetItemQuantity(ResourceType resourceType)
     {
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.item is ResourceItem)
               {
                    ResourceItem resourceItem = (ResourceItem)iStack.item;

                    if (resourceItem.resourceType == resourceType)
                    {
                         return iStack.currentQuantity;
                    }
               }
          }
          return 0;
     }

     public int GetItemQuantity(FoodType foodType)
     {
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.item is FoodItem)
               {
                    FoodItem foodItem = (FoodItem)iStack.item;

                    if (foodItem.foodType == foodType)
                    {
                         return iStack.currentQuantity;
                    }
               }
          }
          return 0;
     }

     public bool IsThereFullItemStack()
     {
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.currentQuantity == iStack.maxQuantity)
               {
                    return true;
               }
          }

          return false;
     }

     public bool IsFoodTypeItemStackFull(FoodType foodType)
     {
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.currentQuantity == iStack.maxQuantity)
               {
                    if(iStack.item is FoodItem)
                    {
                         if(((FoodItem)iStack.item).foodType == foodType) return true;
                    }

                    
               }
          }

          return false;
     }

     public ItemType FullItemStackItemType()
     {
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.currentQuantity == iStack.maxQuantity)
               {
                    if (iStack.item is ResourceItem)
                    {
                         return ItemType.RESOURCE;
                    }
                    else if (iStack.item is FoodItem)
                    {
                         return ItemType.FOOD;
                    }

               }
          }
          return ItemType.NOTHING;
     }

     public void TransferFullItemStackToInventory(Inventory otherInventory, InventoryType otherInventoryType)
     {
          if (otherInventoryType == InventoryType.RESOURCE)
          {
               for (int i = 0; i < resourceTypeEnumLength; i++) //resourceTypeEnumLength
               {
                    if (otherInventory.itemStacks[i].ModifyItemStack(itemStacks[i].currentQuantity))   // Transfer full itemStack from the main inventory to the other
                    {
                         itemStacks[i].currentQuantity = 0;
                    }
               }
          }
          else if (otherInventoryType == InventoryType.FOOD)
          {
               for (int i = resourceTypeEnumLength; i < resourceTypeEnumLength + foodTypeEnumLength; i++) //resourceTypeEnumLength
               {
                    if (otherInventory.itemStacks[i - resourceTypeEnumLength].ModifyItemStack(itemStacks[i].currentQuantity))   // Transfer full itemStack from the main inventory to the other
                    {
                         itemStacks[i].currentQuantity = 0;
                    }
               }
          }

     }

     public int GetItemstackCurrentQuantity(ResourceType resourceType)
     {
          return itemStacks[(int)resourceType].currentQuantity;
     }

     public int GetItemstackCurrentQuantity(FoodType foodType)
     {
          return itemStacks[(int)foodType].currentQuantity;
     }

     
     public string ToString()
     {
          string returnString = "";
          foreach (ItemStack iStack in itemStacks)
          {
               returnString += iStack.ToString() + "   ";
          }
          return returnString;
     }

     public string ToStringNoZeroItems()
     {
          string returnString = "";
          foreach (ItemStack iStack in itemStacks)
          {
               if (iStack.currentQuantity != 0)
               {
                    returnString += iStack.ToString() + "   ";
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
