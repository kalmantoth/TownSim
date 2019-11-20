using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeItemRowScript : MonoBehaviour
{

     public Button PlusButton;
     public Button MinusButton;
     public Text ItemNameText;
     public Text StoredQuantityText;
     public Text ExchangeQuantityText;
     public Text AfterChangeQuantityText;
     public Text TradePriceText;

     private TradeScript tradeScript;
     private Item item;

     private void Start()
     {
          PlusButton.onClick.AddListener(IncreaseItemExchangeQuantity);
          MinusButton.onClick.AddListener(DecreaseItemExchangeQuantity);
     }
     

     public void Init(Item item, TradeScript tradeScript, int exchangeQuantity, int goldCost)
     {
          this.item = item;
          this.tradeScript = tradeScript;

          ItemNameText.text = item.itemName;
          StoredQuantityText.text = item.currentQuantity.ToString();
          ExchangeQuantityText.text = exchangeQuantity.ToString();
          AfterChangeQuantityText.text = (item.currentQuantity + exchangeQuantity).ToString();
          TradePriceText.text = goldCost.ToString();
          this.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
     }

     private void IncreaseItemExchangeQuantity()
     {
          tradeScript.ModifyExchangeValue(this.item, 1);
     }

     private void DecreaseItemExchangeQuantity()
     {
          tradeScript.ModifyExchangeValue(this.item, -1);
     }


}
