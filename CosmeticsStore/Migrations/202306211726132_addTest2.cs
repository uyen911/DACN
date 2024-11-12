namespace CosmeticsStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTest2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_BookingDetails", "BranchId", c => c.Int(nullable: true));
        }

        public override void Down()
        {
            
        }
    }
}
