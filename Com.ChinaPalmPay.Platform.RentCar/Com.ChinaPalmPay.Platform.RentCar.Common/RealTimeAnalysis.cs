﻿using Com.ChinaPalmPay.Platform.RentCar.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.ChinaPalmPay.Platform.RentCar.Common
{
    public class RealTimeAnalysis
    {
        //解析输入流
        public static RunRealTime analysisRun(Stream input)
        {
            //如果有数据可读
            RunRealTime run = new RunRealTime();
            byte[] src = new byte[33];
            readStream(input, src);
            //3字节ID
            byte[] id = new byte[3];
            Array.Copy(src, 0, id, 0, id.Length);
            run.TerminalId = ByteUtils.byteToHexStr(id);
            //4字节纬度--6  31 15.12 34 50
            byte[] lat = new byte[5];
            Array.Copy(src, 3, lat, 0, lat.Length);
            //lat转换int 直接放大10^6倍取整传输 
            run.latitude = long.Parse("" + lat[0] + lat[1] + lat[2] + lat[3] + lat[4]);
            //run.latitude = ((lat[0] << 24) & 0xff000000) + ((lat[1] << 16) & 0xff0000) + ((lat[2] << 8) & 0xff00) + (lat[3] & 0xff);
            //经度--6 120 10 .56 78 90
            byte[] lon = new byte[5];
            Array.Copy(src, 3 + 5, lon, 0, lon.Length);
            // run.longitude = ((lon[0] << 24) & 0xff000000) + ((lon[1] << 16) & 0xff00000) + ((lon[2] << 8) & 0xff00) + (lon[3] & 0xff);
            run.longitude = long.Parse("" + lon[0] + lon[1] + lon[2] + lon[3] + lon[4]);
            //Power 1byte
            byte[] power = new byte[1];
            Array.Copy(src, 3 + 5 + 5, power, 0, power.Length);
            run.batteryInfo = power[0];
            //voltage 1+1byte
            byte[] voltage = new byte[2];
            Array.Copy(src, 3 + 5 + 5 + 1, voltage, 0, voltage.Length);
            run.voltage = ((voltage[0] << 8) & 0xff00) + (voltage[1] & 0xff);
            //current 1+1byte
            byte[] current = new byte[2];
            Array.Copy(src, 3 + 5 + 5 + 1 + 2, current, 0, current.Length);
            run.current = ((current[0] << 8) & 0xff00) + (current[1] & 0xff);
            //temper 1+1byte
            byte[] temper = new byte[2];
            Array.Copy(src, 3 + 5 + 5 + 1 + 2 + 2, temper, 0, temper.Length);
            run.Temper = ((temper[0] << 8) & 0xff00) + (temper[1] & 0xff);
            //门状态：前门1111开  0000关 后门1111开  0000关
            byte[] gate = new byte[1];
            Array.Copy(src, 3 + 5 + 5 + 1 + 2 + 2 + 2, gate, 0, gate.Length);
            run.beforeGateStatus = ((gate[0] >> 4) & 0x0f);
            run.behindGateStatus = ((gate[0]) & 0x0f);
            //车速 1+4byte 车速+续航
            byte[] speed = new byte[5];
            Array.Copy(src, 3 + 5 + 5 + 1 + 2 + 2 + 2 + 1, speed, 0, speed.Length);
            run.speed = speed[0];
            run.mile = ((speed[1] << 24) & 0xff000000) + ((speed[2] << 16) & 0xff0000) + ((speed[3] << 8) & 0xff00) + (speed[4] & 0xff);
            //时间
            byte[] date = new byte[7];
            Array.Copy(src, 3 + 5 + 5 + 1 + 2 + 2 + 2 + 1 + 5, date, 0, date.Length);
            run.sampleTime = dateAnalisis(date, 0);
            return run;
        }
        public static int readStream(Stream input, byte[] src)
        {
            int offset = 0;
            int result = 0;
            int cnt = 0;
            while ((result = input.ReadByte()) != -1)
            {
                cnt++;
                src[offset] = (byte)result;
                offset++;
            }
            return cnt;

        }
        //Return 年月日时分秒时间转换
        public static string dateAnalisis(byte[] date, int offset)
        {
            //年 2byte
            string dates = "";
            dates += (int)((date[offset] << 8) & 0xff00 + (date[offset + 1]) & 0xff);
            //月 1byte
            dates += date[offset + 2] > 9 ? date[offset + 2].ToString() : (string)("0" + date[offset + 2]);
            //日 1byte
            dates += date[offset + 3] > 9 ? date[offset + 3].ToString() : (string)("0" + date[offset + 3]);
            //时 1byte
            dates += date[offset + 4] > 9 ? date[offset + 4].ToString() : (string)("0" + date[offset + 4]);
            //分 1byte
            dates += date[offset + 5] > 9 ? date[offset + 5].ToString() : (string)("0" + date[offset + 5]);
            //秒 1byte
            dates += date[offset + 6] > 9 ? date[offset + 6].ToString() : (string)("0" + date[offset + 6]);
            return dates;
        }
        public static StopRealTime analysisStop(Stream input)
        {
            StopRealTime stop = new StopRealTime();
            byte[] src = new byte[10];
            readStream(input, src);
            //3字节ID
            byte[] id = new byte[3];
            Array.Copy(src, 0, id, 0, id.Length);
            stop.TerminalId = ByteUtils.byteToHexStr(id);
            //1字节电量 0-100
            byte[] power = new byte[1];
            Array.Copy(src, 3, power, 0, power.Length);
            stop.Power = power[0].ToString();
            //2字节电压 0-100
            byte[] voltage = new byte[2];
            Array.Copy(src, 3 + 1, voltage, 0, voltage.Length);
            stop.Voltage = (((voltage[0] << 8) & 0xff00) + (voltage[1] & 0xff)).ToString();
            //2字节电流 0-100
            byte[] current = new byte[2];
            Array.Copy(src, 3 + 1 + 2, current, 0, current.Length);
            stop.Current = (((current[0] << 8) & 0xff00) + (current[1] & 0xff)).ToString();
            //2字节电池温度 0-100
            byte[] temperature = new byte[2];
            Array.Copy(src, 3 + 1 + 2 + 2, temperature, 0, temperature.Length);
            stop.Temperature = temperature[0].ToString();
            return stop;

        }
        public static AuthorizationRequest analysisAuthorizationReq(Stream input)
        {
            AuthorizationRequest request = new AuthorizationRequest();
            byte[] src = new byte[11];
            readStream(input, src);
            //3字节ID
            byte[] id = new byte[3];
            Array.Copy(src, 0, id, 0, id.Length);
            request.TerminalId = ByteUtils.byteToHexStr(id);
            //1字节电量 0-100
            byte[] power = new byte[1];
            Array.Copy(src, 3, power, 0, power.Length);
            request.Power = power[0].ToString();
            //2字节电压 0-100
            byte[] voltage = new byte[2];
            Array.Copy(src, 3 + 1, voltage, 0, voltage.Length);
            request.Voltage = (((voltage[0] << 8) & 0xff00) + (voltage[1] & 0xff)).ToString();
            //1字节电流 0-100
            byte[] current = new byte[2];
            Array.Copy(src, 3 + 1 + 2, current, 0, current.Length);
            request.Current = (((current[0] << 8) & 0xff00) + (current[1] & 0xff)).ToString();
            //1字节电池温度 0-100
            byte[] temperature = new byte[2];
            Array.Copy(src, 3 + 1 + 2 + 2, temperature, 0, temperature.Length);
            request.Temperature = temperature[0].ToString();
            byte[] info = new byte[1];
            Array.Copy(src, 3 + 1 + 2 + 2 + 2, info, 0, info.Length);
            request.updateAuthorizationRequest = info[0].ToString();
            return request;
        }
        //平台下发授权应答编码
        public static byte[] encodeAuthorizationResponse(AuthorizationResponse response)
        {
            //userId:16byte  carId：16byte  time：7byte
            byte[] dst = new byte[16 + 16 + 7];
            byte[] userId = new byte[16];
            userId = ByteUtils.strToToHexByte(response.UserId);
            byte[] carId = new byte[16];
            carId = ByteUtils.strToToHexByte(response.CarId);
            ////授权手机号码 6byte 0x01 0x51 0x51 0x00 0x35 0x47 
            //byte[] tel = new byte[6];
            //string tele = response.Telephone;
            ////手机号码第一个字节
            //byte[] position1 = ByteUtils.strToToHexByte("0" + tele.Substring(0, 1));
            //Array.Copy(position1, 0, tel, 0, 1);
            ////手机号码第二个字节
            //byte[] position2 = ByteUtils.strToToHexByte(tele.Substring(1, 2));
            //Array.Copy(position2, 0, tel, 0 + 1, 1);
            ////手机号码第三个字节
            //byte[] position3 = ByteUtils.strToToHexByte(tele.Substring(3, 2));
            //Array.Copy(position3, 0, tel, 0 + 1 + 1, 1);
            ////手机号码第四个字节
            //byte[] position4 = ByteUtils.strToToHexByte(tele.Substring(5, 2));
            //Array.Copy(position4, 0, tel, 0 + 1 + 1 + 1, 1);
            ////手机号码第五个字节
            //byte[] position5 = ByteUtils.strToToHexByte(tele.Substring(7, 2));
            //Array.Copy(position5, 0, tel, 0 + 1 + 1 + 1 + 1, 1);
            ////手机号码第六个字节
            //byte[] position6 = ByteUtils.strToToHexByte(tele.Substring(9, 2));
            //Array.Copy(position6, 0, tel, 0 + 1 + 1 + 1 + 1 + 1, 1);
            //时间编码
            byte[] time = new byte[7];
            encodeDate(response.SampleTime, time);
            Array.Copy(userId, 0, dst, 0, userId.Length);
            Array.Copy(carId, 0, dst, userId.Length, carId.Length);
            Array.Copy(time, 0, dst, userId.Length + carId.Length, time.Length);
            return dst;
        }
        public static void encodeDate(string time, byte[] dst)
        {
            //2015 ---2byte    02  01 03 22 55--1byte
            int year = int.Parse(time.Substring(0, 4));
            dst[0] = (byte)((year >> 8) & 0xff);
            dst[1] = (byte)(year & 0xff);
            int month = int.Parse(time.Substring(4, 2));
            dst[2] = (byte)(month & 0xff);
            int date = int.Parse(time.Substring(4 + 2, 2));
            dst[3] = (byte)(date & 0xff);
            int hour = int.Parse(time.Substring(4 + 2 + 2, 2));
            dst[4] = (byte)(hour & 0xff);
            int minute = int.Parse(time.Substring(4 + 2 + 2 + 2, 2));
            dst[5] = (byte)(minute & 0xff);
            int second = int.Parse(time.Substring(4 + 2 + 2 + 2 + 2, 2));
            dst[6] = (byte)(second & 0xff);
        }
        //开关门上传数据解析
        public static OpenOrCloseGateRequest openOrCloseGate(Stream input)
        {
            OpenOrCloseGateRequest req = new OpenOrCloseGateRequest();
            req.Id = Guid.NewGuid();
            byte[] src = new byte[42];
            int count = readStream(input, src);
            //3字节ID
            byte[] id = new byte[3];
            Array.Copy(src, 0, id, 0, id.Length);
            string terminalId = ByteUtils.byteToHexStr(id);
            req.TerminalId = terminalId;
            //32字节用户id
            byte[] userId = new byte[32];
            Array.Copy(src, id.Length, userId, 0, userId.Length);
            string dststr = "";
            for (int i = 0; i < userId.Length; i++)
            {
               string c=((char)userId[i]).ToString();
               dststr = dststr + c;
               // string str = System.Text.Encoding.ASCII.GetString(userId);
            }
            //byte[] srcUserId=new byte[16];
            // Array.Copy(userId, 0, srcUserId, 0, srcUserId.Length);
            // string hexstr = ByteUtils.byteToHexStr(userId);
             //string dststr = "";
             //for (int i = 0; i < hexstr.Length; i = i + 2)
             //{
             //    dststr = dststr + hexstr.Substring(i + 1, 1);
             //}
             //req.UserId = dststr;
            req.UserId = dststr;

            byte[] year = new byte[2];
            Array.Copy(src, id.Length + userId.Length, year, 0, year.Length);
            byte[] month = new byte[1];
            Array.Copy(src, id.Length + userId.Length + year.Length, month, 0, month.Length);
            //日 1byte
            byte[] d = new byte[1];
            Array.Copy(src, id.Length + userId.Length + year.Length + month.Length, d, 0, d.Length);
            //时 1byte
            byte[] hour = new byte[1];
            Array.Copy(src, id.Length + userId.Length + year.Length + month.Length + d.Length, hour, 0, hour.Length);
            //分 1byte
            byte[] min = new byte[1];
            Array.Copy(src, id.Length + userId.Length + year.Length + month.Length + d.Length + hour.Length, min, 0, min.Length);
            //秒
            byte[] sec = new byte[1];
            Array.Copy(src, id.Length + userId.Length + year.Length + month.Length + d.Length + hour.Length + min.Length, sec, 0, sec.Length);
            req.SampleTime = "" + ByteUtils.byteToHexStr(year) + ByteUtils.byteToHexStr(month) + ByteUtils.byteToHexStr(d) + ByteUtils.byteToHexStr(hour) + ByteUtils.byteToHexStr(min) + ByteUtils.byteToHexStr(sec);
            return req;
        }
    }
}
