// Book book1 = new Book("Buku 1", "George Orwell", "1949");
// Member anggota1 = new Member(1, "Alice");
// Member anggota2 = new Member(2, "Bob");

// book1.DisplayInfo();
// anggota1.DisplayInfo();

// anggota1.BorrowBook(book1);
// book1.DisplayInfo();
// anggota1.DisplayInfo();
// anggota2.BorrowBook(book1);

int[] arr = { 5, 3, 8, 1, 2 };

int[] result = arr[1..3];
int last2index = arr[^2];
Console.WriteLine(last2index);
Console.WriteLine(string.Join(", ", result));