namespace Messangers.ModelData
{
    public class ModelDataRegister
    {
        public string Tnumber { get; set;}
        public string Mail { get; set; }
        public string Login { get; set; }
        public ReadOnlyMemory<byte> Data { get; set; }
        public string Password { get; set; }
        public DateTime datetime { get; set; }
    }
}
