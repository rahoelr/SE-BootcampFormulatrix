public class Mahasiswa
{
    public int Id { get; set; }
    public string Nama { get; set; }
    public string Jurusan { get; set; }
    public string Semester { get; set; }
    public List<Matakuliah> listMatakuliah = new List<Matakuliah>();

    public Mahasiswa(int id, string nama, string jurusan, string semester)
    {
        this.Id = id;
        this.Nama = nama;
        this.Jurusan = jurusan;
        this.Semester = semester;
    }


}