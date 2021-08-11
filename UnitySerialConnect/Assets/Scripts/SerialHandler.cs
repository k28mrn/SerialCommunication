using System;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;

public class SerialHandler : MonoBehaviour
{
	public delegate void SerialDataReceivedEventHandler(string message);
	public event SerialDataReceivedEventHandler OnDataReceived;

	//ポート名
	//例
	//Linuxでは/dev/ttyUSB0
	//windowsではCOM1
	//Macでは/dev/tty.usbmodem1421など
	public string portName = "/dev/tty.usbmodem";
	public int baudRate    = 9600;

	private SerialPort serialPort_;
	private Thread thread_;
	private bool isRunning_ = false;

	private string message_;
	private bool isNewMessageReceived_ = false;
	
	internal bool isConnected = false;
	float lastedReceivedTime; //最終受信タイム
	float lastedSendTime; //最終送信タイム

	void Awake() {
		Open();
	}

	void OnDestroy() {
		Close();
	}

	/// <summary>
	/// 開始処理
	/// </summary>
	private void Open() {
		string p = portName;

		// Search Port name in all portname
		string [] ports = SerialPort.GetPortNames();

		// Search Port name that matches
		if (Array.IndexOf(ports, p) < 0)
		{
			Regex regex = new Regex("^"+portName);
			foreach (string port in ports) {
				// Debug.Log(port);
				
				if(regex.IsMatch(port)) {
					// Debug.Log("----- matched -----:"+port);
					p = port;
					break;
				}
			}
		}

		Debug.Log("Opening Serial Port: " + p + "／ Baud Rate: " + baudRate.ToString());

		serialPort_ = new SerialPort(p, baudRate, Parity.None, 8, StopBits.One);
		serialPort_.ReadTimeout = 1000;
		
		//または
		//serialPort_ = new SerialPort(portName, baudRate);
		try {
			serialPort_.Open();
			isRunning_ = true;
		} catch (System.Exception e) {
			isRunning_ = false;
			Debug.LogWarning(e.Message);
		}
		thread_ = new Thread(Read);
		thread_.Start();
	}

	void Update() {
		if (isNewMessageReceived_) {
			lastedReceivedTime = Time.time;
			OnDataReceived(message_);
		}
		isNewMessageReceived_ = false;
	}

	/// <summary>
	/// 受信
	/// </summary>
	private void Read() {
		while (isRunning_ && serialPort_ != null && serialPort_.IsOpen) {
			try {
				string message = serialPort_.ReadLine();
				message = message.Replace("\n","").Replace("\r","");
				if (message.Length > 0) {
					message_ = message;
					isNewMessageReceived_ = true;
				}
			} catch (System.Exception e) {
				// NOTE : Time out以外を表示
				if (e.Message != "The operation has timed out.") {
					Debug.LogWarning(e.Message);
				}
			}
		}
	}

	/// <summary>
	/// 送信
	/// </summary>
	/// <param name="msg">送信するメッセージ</param>
	public void Write(string message) {
		try {
			serialPort_.Write(message);
		} catch (System.Exception e) {
			Debug.LogWarning(e.Message);
		}
	}

	/// <summary>
	/// 終了処理
	/// </summary>
	private void Close() {
		isNewMessageReceived_ = false;
		isRunning_ = false;

		if (thread_ != null && thread_.IsAlive) {
			thread_.Join();
		}

		if (serialPort_ != null && serialPort_.IsOpen) {
			serialPort_.Close();
			serialPort_.Dispose();
		}
	}
}