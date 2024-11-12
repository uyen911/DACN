namespace CosmeticsStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTest : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tb_BookingDetails", "BranchId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_BookingDetails", "BranchId", c => c.Int(nullable: true));
        }
    }
}
