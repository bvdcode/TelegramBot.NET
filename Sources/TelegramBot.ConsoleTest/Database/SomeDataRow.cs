using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBot.ConsoleTest.Database
{
    [Table("some_data_rows")]
    public class SomeDataRow
    {
        [Column("id")]
        public int Id { get; set; }
    }
}