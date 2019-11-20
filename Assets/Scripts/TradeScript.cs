﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeScript : MonoBehaviour
{

     private class TradeItemValue
     {
          public int tradeItemQuantityModification;
          public int tradeItemValueModification;

     }

     public GameObject TradeItemRowPrefab;
     public GameObject TradePanelScrollViewContent;
     public SimpleObjectPoolScript tradeItemRowPool;

     public Text TotalGoldSumValueText;
     public Button AcceptTradeButton;
     private int tradeGoldSum;

     private List<Item> tradeItems;
     private List<TradeItemValue> tradeItemValues;

     //private float tradePanelRefreshTimer, tradePanelRefreshTimerInit;

     
     // Start is called before the first frame update
     void Start()
     {
          AcceptTradeButton.onClick.AddListener(AcceptTrade);

          tradeItems = new List<Item>();
          tradeItemValues = new List<TradeItemValue>();

          //tradePanelRefreshTimer = tradePanelRefreshTimerInit = 0.1f;
          InitTradeItems();
     }

     void Update()
     {
          /*
          tradePanelRefreshTimer -= Time.deltaTime;
          if (GlobVars.isTradingPanelOpen && tradePanelRefreshTimer < 0)
          {
               tradePanelRefreshTimer = tradePanelRefreshTimerInit;
          }
          */

          RefreshTradeItemsFromStoredItems();
          
     }

     
     
     private void InitTradeItems()
     {
          foreach(Item item in GlobVars.GetStoredItems())
          {
               
               tradeItems.Add(new Item(item));
               TradeItemValue tradeItemValue = new TradeItemValue();
               tradeItemValue.tradeItemQuantityModification = 0;
               tradeItemValue.tradeItemValueModification = 0;
               tradeItemValues.Add(tradeItemValue);
          }

          Debug.Log("After INIT");
          Debug.Log("GlobVars.GetStoredItems(): " + GlobVars.GetStoredItems().Length);
          Debug.Log("tradeItems.Count: " + tradeItems.Count);
          Debug.Log("Trade panel rows have been initalized...");
          UpdateTradePanel();

     }




     private void RefreshTradeItemsFromStoredItems()
     {

          bool tradeItemsUpToDate = true;

          for (int i = 0; i < GlobVars.GetStoredItems().Length; i++)
          {
               /*
               if (GlobVars.GetStoredItems()[i].currentQuantity != 0)
               {
                    Debug.Log("Store: " + GlobVars.GetStoredItems()[i].ToString()); 
                    Debug.Log("Trade: " + tradeItems[i].ToString());
                    Debug.Log("tradeItems[i].Equals(GlobVars.GetStoredItems()[i]) " + tradeItems[i].Equals(GlobVars.GetStoredItems()[i]));
               }
               */
               if (!tradeItems[i].Equals(GlobVars.GetStoredItems()[i]))
               {
                    tradeItemsUpToDate = false;
                    tradeItems[i] = new Item(GlobVars.GetStoredItems()[i]);
               }

          }
          
          if (!tradeItemsUpToDate)
          {
               Debug.Log("Trade panel items have been updated...");
               UpdateTradePanel();
          }
              
     }

     public void AddTradeItemRow()
     {
          GameObject tradeItem;
          int quantityMod;
          int valueMod;
          TradeItemRowScript tradeItemRowScript = null;

          RefreshTradeItemsFromStoredItems();
          for (int i = 0; i < tradeItems.Count; i++)
          {
               tradeItem = tradeItemRowPool.GetObject();
               tradeItem.transform.SetParent(TradePanelScrollViewContent.transform);
               tradeItemRowScript = tradeItem.GetComponent<TradeItemRowScript>();
               tradeItemRowScript.MinusButton.interactable = tradeItemRowScript.PlusButton.interactable = true;

               //Debug.Log("Trade: " + tradeItems[i].itemName + " " + tradeItems[i].currentQuantity + " ---------------------------");

               quantityMod = tradeItemValues[i].tradeItemQuantityModification;
               valueMod = tradeItemValues[i].tradeItemValueModification;
               
               if ((tradeItems[i].currentQuantity + quantityMod)  <= 0)
               {
                    tradeItemRowScript.MinusButton.interactable = false;
               }
               else if ((tradeItems[i].currentQuantity + quantityMod) >= tradeItems[i].maxQuantity)
               {
                    tradeItemRowScript.PlusButton.interactable = false;
               }
               
               tradeItemRowScript.Init(tradeItems[i], this , quantityMod, valueMod);

          }
     }

     public void RemoveTradeItemRow()
     {
          while (TradePanelScrollViewContent.transform.childCount > 0)
          {
               GameObject toRemove = transform.GetChild(0).gameObject;
               tradeItemRowPool.ReturnObject(toRemove);
          }
     }

     private void RefreshTradeGoldSum()
     {
          int goldSum = 0;
          foreach(TradeItemValue value in tradeItemValues)
          {
               goldSum += value.tradeItemValueModification;
          }

          tradeGoldSum = goldSum;
          TotalGoldSumValueText.text = goldSum.ToString();

          Debug.Log("GlobVars.GOLD + tradeGoldSum: " + (GlobVars.GOLD - tradeGoldSum));
          if ((GlobVars.GOLD + tradeGoldSum) < 0) AcceptTradeButton.interactable = false;
          else AcceptTradeButton.interactable = true;
     }

     public void UpdateTradePanel()
     {
          Debug.Log("Trade panel rows are refreshed...");
          RemoveTradeItemRow();
          AddTradeItemRow();
          RefreshTradeGoldSum();
     }

     public void ResetTradeItemValues()
     {
          foreach (TradeItemValue value in tradeItemValues)
          {
               value.tradeItemQuantityModification = 0;
               value.tradeItemValueModification = 0;
          }
          UpdateTradePanel();
     }

     private void AcceptTrade()
     {
          for (int i = 0; i < tradeItemValues.Count; i++)
          {
               if(tradeItemValues[i].tradeItemQuantityModification != 0)
               {
                    GlobVars.ModifyStoredItemQuantity(tradeItems[i].itemType, tradeItemValues[i].tradeItemQuantityModification);
               }

          }
          GlobVars.GOLD += tradeGoldSum;
          Debug.Log("Trade Completed...");


          ResetTradeItemValues();
          RefreshTradeGoldSum();
          UpdateTradePanel();

          /*
          */
     }



     public void ModifyExchangeValue(Item item, int value)
     {
          int index = tradeItems.IndexOf(item);
          tradeItemValues[index].tradeItemQuantityModification += value;
          tradeItemValues[index].tradeItemValueModification = tradeItemValues[index].tradeItemQuantityModification * -tradeItems[index].value;
          UpdateTradePanel();
     }


     

}
