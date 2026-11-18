namespace StoreApi.Infra.Models {
    public class Book {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
