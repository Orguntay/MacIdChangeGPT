using MacIdChangeGPT.Busines;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Principal;

Yoneticizni();

ChangeMacId();
PhysicalAddress adres = GetMacAddress();
//Console.ReadLine();

void Yoneticizni()
{
    if (!Yoneticiznikontrol())
    {
        ProcessStartInfo program = new ProcessStartInfo();
        program.UseShellExecute = true;
        program.WorkingDirectory = Environment.CurrentDirectory;
        program.FileName = Assembly.GetEntryAssembly().CodeBase;

        program.Verb = "runas";

        try
        {
            Process.Start(program);
            Environment.Exit(0);
        }
        catch (Exception)
        {
            Debug.WriteLine("Program yönetici izni olmadan düzgün çalışmayacaktır!\n yinede çalıştırmak istiyor musunuz ? ");
        }
    }
}

bool Yoneticiznikontrol()
{
    WindowsIdentity id = WindowsIdentity.GetCurrent();
    WindowsPrincipal principal = new WindowsPrincipal(id);

    return principal.IsInRole(WindowsBuiltInRole.Administrator);
}

void ChangeMacId()
{
    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
    PhysicalAddress[] addresses = StoreNetworkInterfaceAddresses();

    PhysicalAddress afterPshcycalAddress = GetMacAddress();

    //NetworkInterface nic = nics.FirstOrDefault(x => x.Name == "Wi-Fi" && x.OperationalStatus == OperationalStatus.Up);
    //PhysicalAddress firstAddress = nic.GetPhysicalAddress();
    string newMacAddress = Convert.ToHexString(GenerateRandomBytes(6));
    bool isChangedMacId = Mac.SetMAC(newMacAddress);

    if (isChangedMacId)
    {
        Console.WriteLine("MAC adresi başarıyla değiştirildi!");
        Debug.WriteLine("Başarılı işlem. MAC adresi başarıyla değiştirildi!");
    }
    PhysicalAddress beforePshcycalAddress = GetMacAddress();
}

static byte[] GenerateRandomBytes(int length)
{
    byte[] randomBytes = new byte[length];

    Random random = new Random();
    for (int i = 0; i < length; i++)
    {
        randomBytes[i] = (byte)random.Next(256);
    }
    return randomBytes;
}

static PhysicalAddress[] StoreNetworkInterfaceAddresses()
{
    IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
    if (nics == null || nics.Length < 1)
    {
        Console.WriteLine("  No network interfaces found.");
        return null;
    }

    PhysicalAddress[] addresses = new PhysicalAddress[nics.Length];
    int i = 0;
    foreach (NetworkInterface adapter in nics)
    {
        IPInterfaceProperties properties = adapter.GetIPProperties();
        PhysicalAddress address = adapter.GetPhysicalAddress();
        byte[] bytes = address.GetAddressBytes();
        PhysicalAddress newAddress = new(bytes);
        addresses[i++] = newAddress;
    }
    return addresses;
}

//static string GetMacAddress()
//{
//    string macAddresses = "";

//    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
//    {
//        if (nic.OperationalStatus == OperationalStatus.Up)
//        {
//            macAddresses += nic.GetPhysicalAddress().ToString();
//            break;
//        }
//    }
//    return macAddresses;
//}
//static string GetMacAddress()
//{
//    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
//    String sMacAddress = string.Empty;
//    foreach (NetworkInterface adapter in nics)
//    {
//        if (sMacAddress == String.Empty)// only return MAC Address from first card
//        {
//            IPInterfaceProperties properties = adapter.GetIPProperties();
//            sMacAddress = adapter.GetPhysicalAddress().ToString();
//        }
//    }
//    return sMacAddress;
//}

static PhysicalAddress GetMacAddress()
{
    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
    {
        // Only consider Ethernet network interfaces
        if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
            nic.OperationalStatus == OperationalStatus.Up)
        {
            return nic.GetPhysicalAddress();
        }
    }
    return null;
}
