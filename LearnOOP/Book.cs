public class Book : IInfo
{
    private string title;
    private string author;
    private string yearPublished;
    private bool status;
    private int bookStock;

    public bool IsAvailable => !status;

    public Book(string title, string author, string yearPublished)
    {
        this.title = title;
        this.author = author;
        this.yearPublished = yearPublished;
        this.status = false;
    }

    public virtual void DisplayInfo()
    {
        if (status == true)
        {
            Console.WriteLine("buku sedang dipinjam");
        }
        else
        {
            Console.WriteLine("buku masih tersedia");
        }
        Console.WriteLine($"Title : " + title + "\nauthor: " + author + "\nYear : " + yearPublished + "\nStatus: " + status);
    }

    public void Borrow()
    {
        status = true;
    }

    public void Return()
    {
        status = false;
    }
}