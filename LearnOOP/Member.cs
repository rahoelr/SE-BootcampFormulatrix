public class Member : Person
{
    int totalBookBorrowed;

    public Member(int id, string name) : base(id, name)
    {
        this.totalBookBorrowed = 0;
    }

    public override void DisplayInfo()
    {
        Console.WriteLine($"ID : " + id + "\nName : " + name + "\nTotal Book Borrowed: " + totalBookBorrowed);
    }

    public void BorrowBook(Book book)
    {
        if (book.IsAvailable)
        {
            book.Borrow();
            totalBookBorrowed++;
        }
        else
        {
            Console.WriteLine("Buku tidak tersedia untuk dipinjam");
        }
    }

    public void ReturnBook(Book book)
    {
        if (!book.IsAvailable)
        {
            book.Return();
            totalBookBorrowed--;
        }
        else
        {
            Console.WriteLine("Buku tidak sedang dipinjam");
        }
    }
}