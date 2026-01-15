public class Mahasiswa
{
    public int Id { get; set; }
    public string Nama { get; set; }
    public string Jurusan { get; set; }

    public Mahasiswa(int id, string nama, string jurusan)
    {
        this.Id = id;
        this.Nama = nama;
        this.Jurusan = jurusan;
    }


}