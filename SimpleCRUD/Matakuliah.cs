public class Matakuliah
{
    public int Id { get; set; }
    public string Nama { get; set; }
    public int SKS { get; set; }

    public Matakuliah(int id, string nama, int sks)
    {
        this.Id = id;
        this.Nama = nama;
        this.SKS = sks;
    }
}