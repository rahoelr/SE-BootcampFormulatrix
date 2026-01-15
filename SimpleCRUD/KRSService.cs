public class KRSService
{
    public void AddMatakuliah(Mahasiswa mhs, Matakuliah mk)
    {
        mhs.listMatakuliah.Add(mk);
    }

    public void RemoveMatakuliah(Mahasiswa mhs, Matakuliah mk)
    {
        mhs.listMatakuliah.Remove(mk);
    }

    public void DisplayKRS(Mahasiswa mhs)
    {
        Console.WriteLine($"KRS Mahasiswa: {mhs.Nama}");
        foreach (var mk in mhs.listMatakuliah)
        {
            Console.WriteLine($"- {mk.Nama} ({mk.SKS} SKS)");
        }

        int totalSks = mhs.listMatakuliah.Sum(m => m.SKS);
        Console.WriteLine($"Total SKS: {totalSks}");
    }
}