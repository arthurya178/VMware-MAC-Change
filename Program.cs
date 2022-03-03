using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Collections;
using System.Threading;

namespace VMwareMacEditor
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //程式參數部分
            string targetIp = "10.0.1";     
            //請輸入 至前3份即可
            string virtualMachineLocation_CentOS = @"C:\Users\E901-61\Documents\Virtual Machines";                             //VMware CentOS虛擬機資料夾位置
            string machineName_CentOS = @"Ubuntu 64-bit 18.04";                                             //虛擬機名稱 CentOS

            string virtualMachineLocation_Ubuntu18_04 = @"C:\Users\E901-61\Documents\Virtual Machines";                             //UBUNTU 虛擬機資料夾位置
            string machineName_Ubuntu18_04 = @"Ubuntu 64-bit 18.04 LTS";                                                           //虛擬機名稱 Ubuntu

             string virtualMachineLocation_Ubuntu20_04 = @"C:\Users\E901-61\Documents\Virtual Machines";                             //UBUNTU 虛擬機資料夾位置
            string machineName_Ubuntu20_04 = @"Ubuntu 64-bit 20.04 LTS";                                                           //虛擬機名稱 Ubuntu



            //執行
            int restartTime = 0; //無法搜尋到IP次數
            while (true)
            {
                string[] ethernetIp = findLocalIp(targetIp);
                if (ethernetIp[1] == "1")
                {
                    changeVmwareMacAddress(ethernetIp[0], virtualMachineLocation_CentOS, machineName_CentOS, 1);
                    changeVmwareMacAddress(ethernetIp[0], virtualMachineLocation_Ubuntu18_04, machineName_Ubuntu18_04, 2);
                    changeVmwareMacAddress(ethernetIp[0], virtualMachineLocation_Ubuntu20_04, machineName_Ubuntu20_04, 3);
                    Console.WriteLine($"Mac Edited Finish");
                    Thread.Sleep(5000);
                    break;
                }
                else if(restartTime >= 5)
                {
                    Console.WriteLine("No Internet or Ip Found , Programe Shutdown !! ");
                    Thread.Sleep(3000);
                    break;
                }
                else
                {
                    Console.WriteLine("No Ip Found , Restart in 3 Second !! ");
                    Thread.Sleep(3000);
                    restartTime++;
                }
            }
            //Pause
            
        }





        static IPAddress[] callIpAdress()                                   //取得所有IP
        {
            // 取得本機名稱
            String strHostName = Dns.GetHostName();

            // 取得本機的 IpHostEntry 類別實體
            IPHostEntry iphostentry = Dns.GetHostByName(strHostName);

            return iphostentry.AddressList;
        }

        static string[] findLocalIp(string targetIp)                          //回傳找尋到的IP 型態STRING[2] 索引0 :{IP},索引1:{是否有找到IP{0:False,1:True)}
        {
            string[] ipInformation = new string[] { "No ip conform !! ,Check your target IP again", "0"}; 
            string[] ipSplit = targetIp.Split('.');
            IPAddress[] ipAddressArray = callIpAdress();

            foreach (IPAddress address in ipAddressArray)
            {
                Boolean check = true;
                string[] buffer = address.ToString().Split('.');
                //如果查到一份錯誤，則非目標IP
                for(int i = 0; i < 3; i++)
                {
                    if (ipSplit[i] != buffer[i])
                    {
                        check = false;
                        break;
                    }
                }
                //表示已找到目標IP
                if(check)
                {
                    ipInformation[0] = address.ToString();
                    ipInformation[1] = "1";
                    break;
                }
            }
            return ipInformation;
        }



        static string[] openTxtToArray(string path)                 //將VMX內的資料處理成陣列
        {
            string text = File.ReadAllText(path, Encoding.Default);
            string[] newText = text.Split('\n');
            return newText;

        }

        static void changeVmwareMacAddress(string localIp,string path,string fileName,int VM_Number)
        {
            List<int> emptyRow = new List<int>();
            List<int> ethernetNo = new List<int>();
            string[] localIpArray = localIp.Split('.');
            string currentHost = string.Format("{0:00:00}", int.Parse(localIpArray[3])) ;
            //Console.WriteLine($"ethernet[n].address = 00:50:56:00:{currentHost}");
            Action<int> checkEthernetExist = no =>
            {
                if (!(ethernetNo.Contains(no)))
                {
                    ethernetNo.Add(no);
                }
            };
            string folderPath = path + @"\" + fileName;
            //確認檔案存在
            if (File.Exists(folderPath + @"\" + fileName + @".vmx"))
            {
                string[] text = openTxtToArray(folderPath + @"\" + fileName + @".vmx");             //處理資料成陣列
                //刪除指定內容
                for (int i = 0; i < text.Length; i++)                                               //批次搜尋資料
                {
                    if (text[i].IndexOf("generatedAddress") != -1 && text[i].IndexOf("generatedAddressOffset") == -1)
                    {
                        emptyRow.Add(i);
                        checkEthernetExist(int.Parse(text[i][8].ToString()));
                        text[i] = "";
                    }
                    if (text[i].IndexOf("addressType") != -1)
                    {
                        emptyRow.Add(i);
                        checkEthernetExist(int.Parse(text[i][8].ToString()));
                        text[i] = "";
                    }
                    if (text[i].IndexOf("genicAddressOffset") != -1)
                    {
                        emptyRow.Add(i);
                        checkEthernetExist(int.Parse(text[i][8].ToString()));
                        text[i] = "";
                    }
                    if (text[i].IndexOf("genicAddressOffset") != -1)
                    {
                        emptyRow.Add(i);
                        checkEthernetExist(int.Parse(text[i][8].ToString()));
                        text[i] = "";
                    }
                    if (text[i].IndexOf("ethernet") != -1 && text[i].IndexOf("address") != -1)
                    {
                        emptyRow.Add(i);
                        checkEthernetExist(int.Parse(text[i][8].ToString()));
                        text[i] = "";
                    }
                }
                //整理字串陣列
                string[] newText = new string[text.Length + ethernetNo.Count()];
                int newTextTimer = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (emptyRow.Contains(i))
                    {
                        continue;
                    }
                    newText[newTextTimer] = text[i];
                    newTextTimer++;

                }

                for (int i = 0; i < ethernetNo.Count(); i++)
                {
                    string currentPort = string.Format("{0:0}", ethernetNo[i]) + string.Format("{0:0}",VM_Number);
                    Console.WriteLine($"CurrentPort = {currentPort}");
                    Console.WriteLine($"CurrentMAC =  00:50:56:{currentPort}:{currentHost}");
                    Thread.Sleep(1000);
                    newText[newTextTimer++] = $"ethernet{ethernetNo[i]}.addressType = static";
                    newText[newTextTimer++] = $"ethernet{ethernetNo[i]}.address = 00:50:56:{currentPort}:{currentHost}";
                }
                
                //轉換字串陣列成字串

                string finalText = "";
                for (int i = 0; i < newText.Length; i++)
                {
                    if (newText[i] != "" && newText[i] != "\n" && newText[i] != " ")
                    {
                        finalText += newText[i];
                        finalText += '\n';
                    }
                }
                //Console.WriteLine(finalText);
                File.WriteAllText(folderPath + @"\" + fileName + @".vmx", finalText, Encoding.Default);
                //File.WriteAllText(folderPath + @"\" + fileName + @".vmx", finalText, Encoding.GetEncoding("big5"));
            }
            else
            {
                Console.WriteLine("File didn't exist!!");
                Thread.Sleep(1500);
            }
        }

    }
}
