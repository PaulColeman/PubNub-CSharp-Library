using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Web.Script.Serialization;

/**
 * PubNub 3.0 Real-time Push Cloud API
 *
 * @author Stephen Blum
 * @package pubnub
 */
namespace PubNub
{
    internal class PubnubBase 
    {
        private string ORIGIN        = "pubsub.pubnub.com";
        private int    LIMIT         = 1800;
        private string PUBLISH_KEY   = "";
        private string SUBSCRIBE_KEY = "";
        private string SECRET_KEY    = "";
        private bool   SSL           = false;

        public delegate bool Procedure(object message);

        public PubnubBase(
            string publish_key,
            string subscribe_key,
            string secret_key,
            bool ssl_on
            ) {
            this.init( publish_key, subscribe_key, secret_key, ssl_on );
            }

        /**
     * Init
     *
     * Prepare PubNub Class State.
     *
     * @param string Publish Key.
     * @param string Subscribe Key.
     * @param string Secret Key.
     * @param bool SSL Enabled.
     */
        public void init(
            string publish_key,
            string subscribe_key,
            string secret_key,
            bool ssl_on
            ) 
        {
            this.PUBLISH_KEY   = publish_key;
            this.SUBSCRIBE_KEY = subscribe_key;
            this.SECRET_KEY    = secret_key;
            this.SSL           = ssl_on;

            // SSL On?
            if (this.SSL)
                this.ORIGIN = "https://" + this.ORIGIN;
            else
                this.ORIGIN = "http://" + this.ORIGIN;
        }


        /**
     * Publish
     *
     * Send a message to a channel.
     *
     * @param String channel name.
     * @param List<object> info.
     * @return bool false on fail.
     */
        public List<object> Publish( string channel, object message ) 
        {
            var serializer = new JavaScriptSerializer();

            Console.WriteLine("publishing...");

            // Generate String to Sign
            string signature = "0";
            if (this.SECRET_KEY.Length > 0) {
                var string_to_sign = new StringBuilder();
                string_to_sign
                    .Append(this.PUBLISH_KEY)
                    .Append('/')
                    .Append(this.SUBSCRIBE_KEY)
                    .Append('/')
                    .Append(this.SECRET_KEY)
                    .Append('/')
                    .Append(channel)
                    .Append('/')
                    .Append(serializer.Serialize(message));

                // Sign Message
                signature = md5(string_to_sign.ToString());
            }

            // Build URL
            var url = new List<string>
                          {
                              "publish",
                              this.PUBLISH_KEY,
                              this.SUBSCRIBE_KEY,
                              signature,
                              channel,
                              "0",
                              serializer.Serialize(message)
                          };

            // Return JSONArray
            return _request(url);
        }


        /**
     * Subscribe - Private Interface
     *
     * @param string channel name.
     * @param Procedure function callback.
     * @param string timetoken.
     */
        public void Subscribe(
            string    channel,
            Procedure callback,
            ref object timetoken) 
        {
            // Build URL
            var url = new List<string> {"subscribe", this.SUBSCRIBE_KEY, channel, "0", timetoken.ToString()};

            // Wait for Message
            var response = _request(url);

            // Update TimeToken
            if (response[1].ToString().Length  > 0)
                timetoken = (object)response[1];

            // Run user Callback and Reconnect if user permits.
            foreach (object message in (object[])response[0]) 
            {
                if (!callback(message)) return;
            }
        }

        /**
     * Request URL
     *
     * @param List<string> request of url directories.
     * @return List<object> from JSON response.
     */
        private List<object> _request(List<string> url_components) {
            string        temp  = null;
            int           count = 0;
            byte[]        buf   = new byte[8192];
            StringBuilder url   = new StringBuilder();
            StringBuilder sb    = new StringBuilder();

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            // Add Origin To The Request
            url.Append(this.ORIGIN);

            // Generate URL with UTF-8 Encoding
            foreach ( string url_bit in url_components) {
                url.Append("/");
                url.Append(EncodeUrIcomponent(url_bit));
            }

            // Fail if string too long
            if (url.Length > this.LIMIT) 
            {
                var too_long = new List<object> {0, "Message Too Long."};
                return too_long;
            }

            // Create Request
            var request = (HttpWebRequest)WebRequest.Create(url.ToString());

            // Set Timeout
            request.Timeout = 200000;
            request.ReadWriteTimeout = 200000;

            // Receive Response
            var response = (HttpWebResponse)request.GetResponse();
            var resStream = response.GetResponseStream();

            // Read
            do {
                count = resStream.Read( buf, 0, buf.Length );
                if (count != 0) {
                    temp = Encoding.UTF8.GetString( buf, 0, count );
                    sb.Append(temp);
                }
            } while (count > 0);

            // Parse Response
            string message = sb.ToString();

            return serializer.Deserialize<List<object>>(message);
        }

        internal static string EncodeUrIcomponent(string s) 
        {
            var o = new StringBuilder();
            foreach (var ch in s.ToCharArray()) 
            {
                if (IsUnsafe(ch)) 
                {
                    o.Append('%');
                    o.Append(ToHex(ch / 16));
                    o.Append(ToHex(ch % 16));
                }
                else 
                    o.Append(ch);
            }

            return o.ToString();
        }

        private static char ToHex(int ch) 
        {
            return (char)(ch < 10 ? '0' + ch : 'A' + ch - 10);
        }

        private static bool IsUnsafe(char ch) 
        {
            return " ~`!@#$%^&*()+=[]\\{}|;':\",./<>?".IndexOf(ch) >= 0;
        }

        private static string md5(string text) {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = Encoding.Default.GetBytes(text);
            byte[] hash = md5.ComputeHash(data);
            string hexaHash = "";
            foreach (byte b in hash)
            {
                hexaHash += String.Format("{0:x2}", b);
            }
            return hexaHash;
        }
    }
}
 
