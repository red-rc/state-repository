using System;
using System.Threading;

namespace ConsoleApp1
{
    public class MessageHelper
    {
        public string getString(string str)
        {
            Console.WriteLine(str);
            return Console.ReadLine();
        }
        public int getInt(string str)
        {
            return Convert.ToInt32(getString(str));
        }
        public byte getByte(string str)
        {
            return Convert.ToByte(getString(str));
        }
    }

    public interface IWashingMachine
    {
        void StartTimer(int time);
    }
    public class WashingMachine : IWashingMachine
    {
        public string BrandName { get; set; }
        public byte Temperature { get; set; }
        public Timer Timer { get; set; }

        public bool IsTimerElapsed = false;

        public bool isEnabled = false;

        private State state = null;

        public WashingMachine()
        {
            this.TransitionTo(new DisabledState());
            state.Handle();
        }

        public void TransitionTo(State state)
        {
            this.state = state;
            this.state.SetContext(this);
            state.Handle();
        }

        public void StartTimer(int time)
        {
            Timer = new Timer(TimerCallback, null, time, Timeout.Infinite);
        }
        private void TimerCallback(object state)
        {
            IsTimerElapsed = true;
        }
    }
    public class HorizontalWM : WashingMachine
    {
        public HorizontalWM(string name)
        {
            this.BrandName = name;
        }
    }
    public class VerticalWM : WashingMachine
    {
        public VerticalWM(string name)
        {
            this.BrandName = name;
        }
    }
    public interface IState
    {
        void Handle();
    }

    public abstract class State : IState
    {
        public MessageHelper messageHelper = new MessageHelper();
        public WashingMachine washingMachine = null;

        public void SetContext(WashingMachine washingMachine)
        {
            this.washingMachine = washingMachine;
        }

        public abstract void Handle();
    }
    public class DisabledState : State
    {
        public override void Handle()
        {
            if (!this.washingMachine.isEnabled && messageHelper.getString("Чи хотіли б ви увімкнути пральну машину? так або ні").ToLower() == "так")
            {
                this.washingMachine.isEnabled = true;
            }
            if (this.washingMachine.isEnabled)
            {
                this.washingMachine.Temperature = messageHelper.getByte("Напишіть температуру прання:");
                int time = messageHelper.getInt("Напишіть час прання в секундах:");
                washingMachine.StartTimer(time * 1000);
                this.washingMachine.TransitionTo(new EnabledState(DateTime.Now.Second + time));
            }
        }
    }
    public class EnabledState : State
    {
        private int time { get; set; }
        private DateTime timeSecondAgo { get; set; }
        public EnabledState(int time)
        {
            this.time = time;
        }

        public override void Handle()
        {
            while (!this.washingMachine.IsTimerElapsed)
            {
                if (DateTime.Now.Second != timeSecondAgo.Second)
                {
                    if (time - DateTime.Now.Second == 0)
                    {
                        Console.WriteLine("Прання завершено!");
                    }
                    else
                    {
                        Console.WriteLine("Прання буде ще тривати - " + (time - DateTime.Now.Second).ToString() + " секунд");
                    }
                    timeSecondAgo = DateTime.Now;
                }
            }

            if (this.washingMachine.IsTimerElapsed)
            {
                this.washingMachine.Timer = null;
                this.washingMachine.IsTimerElapsed = false;
                this.washingMachine.Temperature = default;
                this.washingMachine.TransitionTo(new WaitingState(messageHelper.getString("Забрати речі? так або ні")));
            }
        }
    }

    public class WaitingState : State
    {
        private string userResponse = "";
        public WaitingState(string userResponse)
        {
            this.userResponse = userResponse;
        }
        public override void Handle()
        {
            if (userResponse == "так".ToLower())
            {
                this.washingMachine.isEnabled = false;
                this.washingMachine.TransitionTo(new DisabledState());
            }
        }
    }
    public interface IWMFabric
    {
        IWashingMachine CreateWM(string name);
    }

    public class HorizontalWMFabric : IWMFabric
    {
        public IWashingMachine CreateWM(string name)
        {
            return new HorizontalWM(name);
        }
    }
    public class VerticalWMFabric : IWMFabric 
    {
        public IWashingMachine CreateWM(string name)
        {
            return new VerticalWM(name);
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            HorizontalWMFabric HWMFabric = new HorizontalWMFabric();
            IWashingMachine horizontalWM = HWMFabric.CreateWM("Bosch");

            VerticalWMFabric VWMFabric = new VerticalWMFabric();
            IWashingMachine verticalWM = VWMFabric.CreateWM("Bosch");
        }
    }
}
