using System.Text;

namespace Lorikeet;
using System.IO.Ports;

public static class Serial {
    private static bool connected = false;
    public static bool Connected => connected;

    private static int attempts = 0;
    
    private static string connect_buffer = "";
    
    public static SerialPort serial_port;
    public static FileStream serial_stream;
    
    public static bool Connect() {
        if (!connected) {
            serial_port = new SerialPort();
            serial_port.BaudRate = 115200;
            
            Logging.Config("Attempting handshake...");
            
            foreach (string pn in SerialPort.GetPortNames()) {
                Logging.Config("Trying " + pn);
                serial_port.PortName = pn;

                try {
                    serial_port.Open();
                    Thread.Sleep(1000);
                    
                    if (serial_port.IsOpen) {
                        Logging.Config("Serial opened, sending PEEPEE");

                        while (attempts < 15) {
                            attempts++;

                            serial_port.Write("PEEPEE/");

                            var start = DateTime.UtcNow;
                            while ((DateTime.UtcNow - start).TotalMilliseconds < 1000) {
                                connect_buffer += serial_port.ReadExisting();
                                Logging.Config(connect_buffer);
                                Thread.Sleep(5);

                                if (connect_buffer.StartsWith("POOPOO")) {
                                    connected = true;
                                    attempts = 0;
                                    connect_buffer = "";

                                    break;
                                }
                            }

                            if (connected) {
                                Logging.Config("Successful PEEPEE POOPOO handshake!");
                                break;
                            }
                        }
                    }

                    attempts = 0;
                    connect_buffer = "";

                    if (!connected) serial_port.Close();

                } catch (System.IO.IOException ex) {
                    attempts = 0;
                    connect_buffer = "";
                    serial_port.Close();
                } catch (System.TimeoutException tex) {
                    attempts = 0;
                    connect_buffer = "";
                    serial_port.Close();
                }

                if (connected) {
                    serial_port.Close();
                    serial_stream = new FileStream(pn, FileMode.Open, FileAccess.ReadWrite,
                        FileShare.None);
                }
            }
        }
        
        if (!connected)
            Logging.Config("Failed to connect!");
        
        return false;
    }

    public static void Disconnect() {
        if (connected) {
            connected = false;
            serial_stream.Close();
        }
    }
}