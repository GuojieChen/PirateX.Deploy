namespace PirateX.Deploy.Command
{
    public class OriginCommand
    {
        public OriginCommand()
        {
            Name = "";
            Param = "";
        }
        public string Name { get; set; }

        public string Param { get; set; }

        public OriginCommand SubCommand { get; set; }
    }
}
