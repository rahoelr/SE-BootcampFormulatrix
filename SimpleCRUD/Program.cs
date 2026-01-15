MahasiswaRepository mhRepo = new MahasiswaRepository();


while (true) {
    Console.WriteLine("Pilih operasi CRUD Mahasiswa: ");
    Console.WriteLine("1. Create Mahasiswa");
    Console.WriteLine("2. Read All Mahasiswa");
    Console.WriteLine("3. Update Mahasiswa");
    Console.WriteLine("4. Delete Mahasiswa");
    Console.WriteLine("5. Exit");
    Console.Write("Masukkan pilihan (1-5): ");
    string choice = Console.ReadLine();
    

    switch (choice)
    {
        case "1":
            Console.Write("Masukkan ID: ");
            int id = int.Parse(Console.ReadLine());
            Console.Write("Masukkan Nama: ");
            string nama = Console.ReadLine();
            Console.Write("Masukkan Jurusan: ");
            string jurusan = Console.ReadLine();

            Mahasiswa newMhs = new Mahasiswa(id, nama, jurusan);
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

            mhRepo.UpdateMahasiswa(updateId, newNama, newJurusan);
            break;
        case "4":
            Console.Write("Masukkan ID Mahasiswa yang akan dihapus: ");
            int deleteId = int.Parse(Console.ReadLine());
            mhRepo.DeleteMahasiswa(deleteId);
            break;
        case "5":
            Console.WriteLine("Exiting program.");
            return;
        default:
            Console.WriteLine("Pilihan tidak valid. Silakan coba lagi.");
            break;
    }
}