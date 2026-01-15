MahasiswaRepository mhRepo = new MahasiswaRepository();
KRSService kRSService = new KRSService();


while (true) {
    Console.WriteLine("Pilih operasi CRUD Mahasiswa: ");
    Console.WriteLine("1. Create Mahasiswa");
    Console.WriteLine("2. Read All Mahasiswa");
    Console.WriteLine("3. Update Mahasiswa");
    Console.WriteLine("4. Delete Mahasiswa");
    Console.WriteLine("5. Add KRS Matakuliah");
    Console.WriteLine("6. Display KRS Mahasiswa");
    Console.WriteLine("7. Exit");
        Console.Write("Masukkan pilihan (1-7): ");
    string choice = Console.ReadLine();
    

    switch (choice)
    {
        case "1":
            Console.Write("Masukkan Nama: ");
            string nama = Console.ReadLine();
            Console.Write("Masukkan Jurusan: ");
            string jurusan = Console.ReadLine();
            Console.Write("Masukkan Semester: ");
            string semester = Console.ReadLine();

            int currentId = mhRepo.getLastId();
            int nextId = currentId + 1; 

            Mahasiswa newMhs = new Mahasiswa(nextId, nama, jurusan, semester);
            mhRepo.addMahasiswa(newMhs);
            Console.WriteLine("Mahasiswa berhasil ditambahkan.");
            break;
        case "2":
            List<Mahasiswa> allMhs = mhRepo.getAllMahasiswa();
            Console.WriteLine("Daftar Mahasiswa:");
            foreach (Mahasiswa mhs in allMhs)
            {
                Console.WriteLine($"ID: {mhs.Id}, Nama: {mhs.Nama}, Jurusan: {mhs.Jurusan}");
            }
            break;
        case "3": 
            Console.Write("Masukkan ID Mahasiswa yang akan diupdate: ");
            int updateId = int.Parse(Console.ReadLine());
            Console.Write("Masukkan Nama baru: ");
            string newNama = Console.ReadLine();
            Console.Write("Masukkan Jurusan baru: ");
            string newJurusan = Console.ReadLine();
            Console.Write("Masukkan Semester baru: ");
            string newSemester = Console.ReadLine();

            mhRepo.UpdateMahasiswa(updateId, newNama, newJurusan, newSemester);
            break;
        case "4":
            if (mhRepo.getAllMahasiswa() == null)
            {
                break;
            }
            foreach (Mahasiswa mhs in mhRepo.getAllMahasiswa())

                {
                    Console.WriteLine($"ID: {mhs.Id}, Nama: {mhs.Nama}, Jurusan: {mhs.Jurusan}");
                }
            Console.Write("Masukkan ID Mahasiswa yang akan dihapus: ");
            int deleteId = int.Parse(Console.ReadLine());
            mhRepo.DeleteMahasiswa(deleteId);
            break;
        case "5":
            Console.WriteLine("Masukkan ID Mahasiswa untuk menambahkan Matakuliah: ");
            int mhsId = int.Parse(Console.ReadLine());
            Mahasiswa mhsData = mhRepo.GetMahasiswaById(mhsId);
            if (mhsData == null)
            {
                break;
            }
            Console.Write("Masukkan Nama Matakuliah: ");
            string mkNama = Console.ReadLine();
            Console.Write("Masukkan SKS Matakuliah: ");
            int mkSks = int.Parse(Console.ReadLine());
            int mkId = mhsData.listMatakuliah.Count + 1;
            Matakuliah newMk = new Matakuliah(mkId, mkNama, mkSks);
            kRSService.AddMatakuliah(mhsData, newMk);
            Console.WriteLine("Matakuliah berhasil ditambahkan ke KRS Mahasiswa.");
            break;
        case "6": 
            Console.Write("Masukkan ID Mahasiswa untuk menampilkan KRS: ");
            int displayMhsId = int.Parse(Console.ReadLine());
            Mahasiswa displayMhsData = mhRepo.GetMahasiswaById(displayMhsId);
            if (displayMhsData == null)
            {
                break;
            }
            kRSService.DisplayKRS(displayMhsData);
            break;
        case "7":
            Console.WriteLine("Exiting program.");
            return;
        default:
            Console.WriteLine("Pilihan tidak valid. Silakan coba lagi.");
            break;
    }
}