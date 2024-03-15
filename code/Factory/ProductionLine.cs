public class ProductionLine : Component
{
    [Property] public bool IsActive { get; set; } = false;
    [Category ("Required")]
    [Property]
    public int Order { get; set; }

    [Category ("Required")]
    [Property]
    public int Price { get; set; }

    [Description ("Dictionary of Upgrader and if it is upgraded")]
    [Property]
    public Dictionary<Upgrader, bool> Upgrader { get; set; }
    
    public GameObject ProductionLineObject { get; set; }

    protected override void OnStart()
    {
        base.OnStart();
        ProductionLineObject = GameObject;
        SetUp();
    }

    public void SetUp()
    {
        // Set the ProductionLineObject to be disabled
        ProductionLineObject.Enabled = false;

        // Set the Upgrader List and sort it
        Upgrader = GameObject.Components.GetAll<Upgrader>(FindMode.EverythingInDescendants).ToDictionary(x => x, x => false);
        Upgrader = Upgrader.OrderBy(x => x.Key.UpgradeOrder).ToDictionary(x => x.Key, x => x.Value);

        // Disable all the upgraders
        foreach (var upgrader in Upgrader)
        {
            upgrader.Key.GameObject.Enabled = false;
        }
    }

    public bool CanUpgrade()
    {
        // If the production line is not active, return false
        if (!IsActive) return false;

        // If there is at least one upgrader that is not upgraded, return true
        foreach (var upgrader in Upgrader)
        {
            if (!upgrader.Value)
            {
                return true;
            }
        }

        return false;
    }

    public void Upgrade()
    {
        // If the production line is not active, return
        if (!IsActive) return;

        // For each upgrader, upgrade the first one that is not upgraded
        foreach (var upgrader in Upgrader)
        {
            if (!upgrader.Value)
            {
                // Upgrade the upgrader
                upgrader.Key.GameObject.Enabled = true;
                Upgrader[upgrader.Key] = true;
                if (upgrader.Key.GameObject.Parent.Children.Count > 0)
			        upgrader.Key.GameObject.Parent.Children.Where(x => x.Tags.Has("conveyor") && !x.Tags.Has("upgrader")).FirstOrDefault().Enabled = false;
                return;
            }
        }
    }

    public bool FullUpgraded()
    {
        // If the production line is not active, return false
        if (!IsActive) return false;

        // If there is at least one upgrader that is not upgraded, return false
        foreach (var upgrader in Upgrader)
        {
            if (!upgrader.Value)
            {
                return false;
            }
        }

        return true;
    }
}