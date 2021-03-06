﻿using Com.ChinaPalmPay.Platform.RentCar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.ChinaPalmPay.Platform.RentCar.IMessaging
{
  public  interface ICupMsmq
    {
        void SendCup(Cup user);
        Cup ReceiveCup();
        void SendRecharge(Recharge user);
        Recharge ReceiveRecharge();
        void SendOrderLog(OrderLog user);
        OrderLog ReceiveOrderLog();
    }
}
