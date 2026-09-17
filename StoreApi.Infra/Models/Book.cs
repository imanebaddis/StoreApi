namespace StoreApi.Infra.Models {
    public class Book {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string? OpenLibraryKey { get; set; }
        public int? FirstPublishYear { get; set; }
        public string? Subject { get; set; }
    }
}
