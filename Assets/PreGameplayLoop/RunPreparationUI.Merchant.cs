using UnityEngine;

namespace RogZombie.PreGameplayLoop
{
    public sealed partial class RunPreparationUI
    {
        private readonly System.Collections.Generic.Dictionary<PrototypeItem, Texture2D> merchantIcons = new System.Collections.Generic.Dictionary<PrototypeItem, Texture2D>();
        private int saleSlot = -1;
        private string merchantMessage = "";
        private static readonly PrototypeItem[] merchantItems = {
            PrototypeItem.Molotov, PrototypeItem.Granata, PrototypeItem.Smoke,
            PrototypeItem.Trappola, PrototypeItem.PozioneCurativa,
            PrototypeItem.BombaVelenosa, PrototypeItem.MinaElettrica
        };

        // Coordinates follow the owner's 1536 x 864 reference, using the existing UI scaling.
        private void DrawMerchant()
        {
            var selection = hub.Selection;
            var account = selection.Merchant;
            Outline(new Rect(16, 5, 212, 55), orange);
            GUI.Label(new Rect(16, 5, 212, 55), account.Gold + " G", centre);
            Outline(new Rect(16, 70, 963, 627), pink);
            GUI.Label(new Rect(180, 90, 650, 65), "MERCHANT", centre);

            // Geometric portrait until the final character art is supplied.
            Fill(new Rect(435, 190, 120, 120), new Color(.68f, .55f, .43f));
            Fill(new Rect(400, 310, 190, 190), new Color(.32f, .38f, .31f));
            Fill(new Rect(310, 465, 375, 36), new Color(.45f, .31f, .2f));
            Outline(new Rect(125, 522, 696, 152), yellow);
            GUI.Label(new Rect(140, 535, 667, 125), merchantMessage, text);

            for (int slot = 0; slot < selection.Items.Length; slot++)
            {
                var rect = new Rect(18 + slot * 130, 725, 112, 116);
                bool occupied = selection.ItemCounts[slot] > 0;
                if (saleSlot == slot && occupied)
                {
                    Outline(rect, green);
                    DrawMerchantIcon(new Rect(rect.x + 5, rect.y + 5, 30, 30), selection.Items[slot]);
                    GUI.Label(new Rect(rect.x + 35, rect.y, rect.width - 35, 38), "x" + selection.ItemCounts[slot], centre);
                    int refund = MerchantAccount.Price(selection.Items[slot]) / 2;
                    if (BoxButton(new Rect(rect.x + 5, rect.y + 39, rect.width - 10, 72), "−1\n+" + refund + " G", orange))
                    {
                        string name = ItemRuntime.Label(selection.Items[slot]);
                        if (account.Sell(selection, slot)) merchantMessage = name + ": venduta 1 unità. +" + refund + " G.";
                        if (selection.ItemCounts[slot] == 0) saleSlot = -1;
                    }
                }
                else
                {
                    if (BoxButton(rect, occupied ? "" : "VUOTO", green)) saleSlot = occupied ? slot : -1;
                    if (occupied)
                    {
                        DrawMerchantIcon(new Rect(rect.x + 16, rect.y + 6, 80, 80), selection.Items[slot]);
                        GUI.Label(new Rect(rect.x + 5, rect.y + 85, rect.width - 10, 26),
                            new GUIContent("x" + selection.ItemCounts[slot], ItemRuntime.Label(selection.Items[slot])),
                            new GUIStyle(centre) { fontSize = 16, padding = new RectOffset() });
                    }
                }
            }
            if (BoxButton(new Rect(605, 744, 319, 91), "INDIETRO", Color.gray))
            { saleSlot = -1; merchantMessage = ""; selection.Back(); }

            Outline(new Rect(996, 28, 527, 815), new Color(0, .62f, .78f));
            for (int i = 0; i < merchantItems.Length; i++)
            {
                var item = merchantItems[i]; int price = MerchantAccount.Price(item);
                float y = 39 + i * 115;
                Outline(new Rect(1015, y, 490, 103), red);
                Outline(new Rect(1015, y, 100, 103), red);
                Outline(new Rect(1115, y, 203, 103), red);
                DrawMerchantIcon(new Rect(1030, y + 5, 70, 70), item);
                GUI.Label(new Rect(1019, y + 71, 92, 27), ItemRuntime.Label(item), new GUIStyle(centre) { fontSize = 10, padding = new RectOffset(), wordWrap = true });
                GUI.Label(new Rect(1115, y, 203, 103), price + " G", centre);
                bool room = account.AvailableSlot(selection, item) >= 0;
                if (BoxButton(new Rect(1318, y, 187, 103), !room ? "SLOT PIENI" : account.Gold < price ? "G INSUFFICIENTI" : "ACQUISTA", red, room && account.Gold >= price))
                {
                    int destination = account.AvailableSlot(selection, item);
                    if (account.Buy(selection, item))
                    { saleSlot = -1; merchantMessage = ItemRuntime.Label(item) + ": acquistata 1 unità nello SLOT " + (destination + 1) + "."; }
                }
            }
        }

        private void DrawMerchantIcon(Rect rect, PrototypeItem item)
        {
            if (!merchantIcons.TryGetValue(item, out var icon))
            {
                string asset;
                switch (item)
                {
                    case PrototypeItem.Molotov: asset = "molotov"; break;
                    case PrototypeItem.Granata: asset = "granata"; break;
                    case PrototypeItem.Smoke: asset = "smoke"; break;
                    case PrototypeItem.PozioneCurativa: asset = "pozione-curativa"; break;
                    case PrototypeItem.Trappola: asset = "trappola"; break;
                    case PrototypeItem.BombaVelenosa: asset = "bomba-velenosa"; break;
                    case PrototypeItem.MinaElettrica: asset = "mina-elettrica"; break;
                    default: return;
                }
                icon = Resources.Load<Texture2D>("MerchantItems/" + asset);
                merchantIcons[item] = icon;
                if (icon == null) Debug.LogWarning("Icona MERCHANT mancante: " + asset, this);
            }
            if (icon != null) GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
            else GUI.Label(rect, ItemRuntime.Label(item), centre);
        }
    }
}
