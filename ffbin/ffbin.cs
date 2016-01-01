using System.ServiceProcess;
using System.Threading;

namespace ffbin
{
    public partial class ffbin : ServiceBase
    {
        public ffbin()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            new Thread(new ThreadStart(() =>
            {
                var f = new FFMPEG();
                f.Update();
            })).Start();
        }

        protected override void OnStop()
        {
            //do nothing
        }

        public void Start()
        {
            OnStart(null);  
        }
    }
}
