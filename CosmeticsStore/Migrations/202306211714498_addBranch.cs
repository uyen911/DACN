namespace CosmeticsStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBranch : DbMigration
    {
            public override void Up()
            {
                AddColumn("dbo.tb_BookingDetails", "BranchId", c => c.Int(nullable: false));
            }

            public override void Down()
            {
            }
    }
}
