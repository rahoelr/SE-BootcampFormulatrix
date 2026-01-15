public class MahasiswaRepository
{
    List<Mahasiswa> listMahasiswa = new List<Mahasiswa>();

    public void addMahasiswa(Mahasiswa mhs)
    {
        listMahasiswa.Add(mhs);
    }

    public List<Mahasiswa> getAllMahasiswa()
    {
        return listMahasiswa;
    }

    public Mahasiswa GetMahasiswaById(int id)
    {
        foreach (Mahasiswa mhs in listMahasiswa)
        {
            if (mhs.Id == id)
            {
                return mhs;
            }
        }
        Console.WriteLine("Data not found.");  
        return null;
    }

    public Mahasiswa UpdateMahasiswa(int id, string nama, string jurusan)
    {
        Mahasiswa data = GetMahasiswaById(id);
        if (data != null)
        {
            data.Nama = nama;
            data.Jurusan = jurusan;
            Console.WriteLine("Data successfully updated.");
            return data;
        } else {
            return null;
        }
    }

    public void DeleteMahasiswa(int id)
    {
        Mahasiswa data = GetMahasiswaById(id);
        if (data != null)
        {
            listMahasiswa.Remove(data);
            Console.WriteLine("Data successfully deleted.");
        } else {
            Console.WriteLine("Data not found.");
        }
    }

}