﻿using Com.Chinapalmpay.Component.Log;
using Com.ChinaPalmPay.Platform.RentCar.BLLFacs;
using Com.ChinaPalmPay.Platform.RentCar.Common;
using Com.ChinaPalmPay.Platform.RentCar.IBLLS;
using Com.ChinaPalmPay.Platform.RentCar.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace com.chinapalmpay.platform.RentCars.Controllers
{
    public class RealTimeController : BaseController
    {
        //
        // GET: /RealTime/
        private static readonly IRealTimeHandler orderbll = BllAccess.CreateRealTimeService();
        public void Run()
        {
            LogerHelper.DefaultInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " RUN");
            Stream inputStream= Request.GetBufferedInputStream();
            RunRealTime real=RealTimeAnalysis.analysisRun(inputStream);
            real.sampleTime = string.Format("{0:yyyyMMddHHmmssfff}", DateTime.Now);
            if (RealTimeThread.dic.ContainsKey(real.TerminalId))
            {
                CarInfo info = RealTimeThread.dic[real.TerminalId] as CarInfo;
                if (info != null)
                {
                    info.Power = real.batteryInfo;
                    info.Voltage = real.voltage;
                    info.Speed = real.speed;
                    info.Mile = real.mile;
                    info.Longitude = real.longitude;
                    info.Latitude = real.latitude;
                    RealTimeThread.dic[real.TerminalId] = info;
                }
            }
            else
            {
                CarInfo c = new CarInfo();
                c.Power = real.batteryInfo;
                c.Voltage = real.voltage;
                c.Speed = real.speed;
                c.Mile = real.mile;
                c.Longitude = real.longitude;
                c.Latitude = real.latitude;
                RealTimeThread.dic.Add(real.TerminalId,c);
            }
           
            if(real!=null){
                orderbll.uploadRunRealTime(real);
            }
        }
        public void Stop()
        {
            LogerHelper.DefaultInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " Stop");
            Stream inputStream = Request.GetBufferedInputStream();
            StopRealTime real = RealTimeAnalysis.analysisStop(inputStream);
            if (RealTimeThread.dic.ContainsKey(real.TerminalId))
            {
                CarInfo info = RealTimeThread.dic[real.TerminalId] as CarInfo;
                if (info != null)
                {
                    info.Power = int.Parse(real.Power);
                    info.Voltage = int.Parse(real.Voltage);
                    info.Current = int.Parse(real.Current);
                    info.Temperature = int.Parse(real.Temperature);
                    RealTimeThread.dic[real.TerminalId] = info;
                }
            }
            else
            {
                CarInfo c = new CarInfo();
                c.Power = int.Parse(real.Power);
                c.Voltage = int.Parse(real.Voltage);
                c.Current = int.Parse(real.Current);
                c.Temperature = int.Parse(real.Temperature);
                RealTimeThread.dic.Add(real.TerminalId,c);

            }
           
            if (real != null)
            {
                orderbll.uploadStopRealTime(real);
            }
        }
        public  void Authorization()
        {
            try
            {
                LogerHelper.DefaultInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 收到终端授权请求");
                Stream inputStream = Request.GetBufferedInputStream();
                Stream outputStream = Response.OutputStream;
                AuthorizationRequest real = RealTimeAnalysis.analysisAuthorizationReq(inputStream);
                if (RealTimeThread.dic.ContainsKey(real.TerminalId))
                {
                    CarInfo info = RealTimeThread.dic[real.TerminalId] as CarInfo;
                    if (info == null)
                    {
                        info.Power = int.Parse(real.Power);
                        info.Voltage = int.Parse(real.Voltage);
                        info.Current = int.Parse(real.Current);
                        info.Temperature = int.Parse(real.Temperature);
                        RealTimeThread.dic[real.TerminalId] = info;
                    }
                }
                else
                {
                    CarInfo c = new CarInfo();
                    c.Power = int.Parse(real.Power);
                    c.Voltage = int.Parse(real.Voltage);
                    c.Current = int.Parse(real.Current);
                    c.Temperature = int.Parse(real.Temperature);
                    RealTimeThread.dic.Add(real.TerminalId, c);
                }

                if (real != null)
                {
                    byte[] outs = orderbll.uploadAuthorization(real);
                    Thread.Sleep(1000);
                    if (outs!=null)
                    {
                    outputStream.WriteByte(0xd3);
                    outputStream.WriteByte(0xd3);
                    outputStream.Write(outs, 0, outs.Length);
                    LogerHelper.DefaultInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 下发终端授权响应" + ByteUtils.byteToHexStr(outs));
                   //修改订单状态为已使用
                    outputStream.Flush();
                    }
                }
                outputStream.Close();
                inputStream.Close();
            }catch(Exception e){
                LogerHelper.exception("error occured!",e);
            }
        }
        public void OpenCloseGate()
        {
            LogerHelper.DefaultInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 收到终端开门请求");
            try
            {
                Stream inputStream = Request.GetBufferedInputStream();
                //byte[] src = new byte[42];
                //int offset = 0;
                //int result = 0;
                //int cnt = 0;
                //while ((result = inputStream.ReadByte()) != -1)
                //{
                //    cnt++;
                //    src[offset] = (byte)result;
                //    offset++;
                //}
               // LogerHelper.debug("平台收到数据："+ByteUtils.byteToHexStr(src));
                OpenOrCloseGateRequest opens = RealTimeAnalysis.openOrCloseGate(inputStream);
                opens.CreateTime = DateTime.Now;
                orderbll.uploadOpenOrCloseGate(opens);
            }catch(Exception e){
                  LogHelper.Exception(this.Url.RequestContext,e); 
            }
        }
        public long ServerTime()
        {
            DateTime oldTime = new DateTime(1970,1,1,0,0,0,0);
            long milliSecondsTime = (long)DateTime.Now.Subtract(oldTime).TotalMilliseconds;
            return milliSecondsTime;
        }
        //查询电池 电压  电量  速度 经纬度等数据
        public ActionResult selectRun()
        {
            RunRealTime real=orderbll.selectRunReal();
            if(real!=null){
                ViewData.Model = real;
            }
            return View();
        }
        public ActionResult selectStop()
        {
            StopRealTime real = orderbll.selectStopReal();
            if (real != null)
            {
                ViewData.Model = real;
            }
            return View();
        }
        public ActionResult selectAutho()
        {
            AuthorizationRequest real = orderbll.selectAuthorizationReal();
            if (real != null)
            {
                ViewData.Model = real;
            }
            return View();
        }
    }
}
