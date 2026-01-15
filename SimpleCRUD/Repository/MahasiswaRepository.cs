public class MahasiswaRepository
{
    List<Mahasiswa> listMahasiswa = new List<Mahasiswa>();

    public void addMahasiswa(Mahasiswa mhs)
    {
        listMahasiswa.Add(mhs);
    }

    public int getLastId()
    {
        if (listMahasiswa.Count == 0)
        {
            return 0;
        } else
        {
            return listMahasiswa[listMahasiswa.Count - 1].Id;
        }
    }

    public List<Mahasiswa> getAllMahasiswa()
    {
        if (listMahasiswa.Count == 0)
        {
            Console.WriteLine("Data Mahasiswa kosong.");
            return null;
        } else
        {
            Console.WriteLine("Data Mahasiswa tersedia.");
            return listMahasiswa;
        }
  
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
            if (nama != "")
            {
                data.Nama = nama;
            }
            if (jurusan != "")
            {
                data.Jurusan = jurusan;
            } 
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