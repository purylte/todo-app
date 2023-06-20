using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class TimeUpdateTodoTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
CREATE FUNCTION ""Todo_Update_Timestamp_Function""() RETURNS TRIGGER LANGUAGE PLPGSQL AS $$
BEGIN
    NEW.""LastUpdated"" := now();
    RETURN NEW;
END;
$$
");           
            
            migrationBuilder.Sql(
                @"
CREATE TRIGGER ""UpdateTimestamp""
    BEFORE INSERT OR UPDATE
    ON ""Todos""
    FOR EACH ROW
    EXECUTE FUNCTION ""Todo_Update_Timestamp_Function""();
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
