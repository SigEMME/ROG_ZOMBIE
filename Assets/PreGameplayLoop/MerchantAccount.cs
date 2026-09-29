namespace RogZombie.PreGameplayLoop
{
    // Session-local test wallet. No QUEST progression or permanent save is implied.
    public sealed class MerchantAccount
    {
        public int Gold { get; private set; } = 5000;
        public static int Price(PrototypeItem item)
        {
            switch (item)
            {
                case PrototypeItem.Molotov: case PrototypeItem.Granata:
                case PrototypeItem.Smoke: case PrototypeItem.Trappola: return 50;
                case PrototypeItem.PozioneCurativa: case PrototypeItem.BombaVelenosa: return 100;
                case PrototypeItem.MinaElettrica: return 150;
                default: return 0;
            }
        }
        public void Credit(int amount) { if (amount > 0) Gold += amount; }
        public int AvailableSlot(RunPreparationSelection selection, PrototypeItem item)
        {
            for (int i = 0; i < selection.Items.Length; i++)
                if (selection.ItemCounts[i] == 0 || selection.Items[i] == item && selection.ItemCounts[i] < selection.ItemCapacity(item)) return i;
            return -1;
        }
        public bool Buy(RunPreparationSelection selection, PrototypeItem item)
        {
            int price = Price(item), slot = AvailableSlot(selection, item);
            if (selection.Page != PreparationPage.Merchant || price <= 0 || Gold < price || slot < 0) return false;
            selection.Items[slot] = item; selection.ItemCounts[slot]++; Gold -= price;
            return true;
        }
        public bool Sell(RunPreparationSelection selection, int slot)
        {
            if (selection.Page != PreparationPage.Merchant || slot < 0 || slot >= selection.Items.Length || selection.ItemCounts[slot] <= 0) return false;
            Credit(Price(selection.Items[slot]) / 2);
            if (--selection.ItemCounts[slot] == 0) selection.Items[slot] = PrototypeItem.None;
            return true;
        }
    }
}
