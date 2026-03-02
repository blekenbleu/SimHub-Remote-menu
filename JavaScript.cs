using System.Text;

namespace blekenbleu.SimHub_Remote_menu
{
	partial class HttpServer	// works in .NET Framework 4.8 WPF User Control library (SimHub plugin)
	{
		static Control View;

/*		Using server-sent events https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events
 ;			https://html.spec.whatwg.org/multipage/server-sent-events.html#server-sent-events
 ;		Custom SSE events
 ;			https://medium.com/pon-tech-talk/a-simple-guide-to-server-sent-events-sse-and-eventsource-9de19c23645b
 ;			https://github.com/omer-pon/sse-eventsource
 ;
 ;		https://www.w3schools.com/Js/js_json_parse.asp
 ;		JavaScript minification:  https://github.com/trullock/NUglify
 ;		JavaScript Debugging:  https://www.w3schools.com/js/js_debugging.asp
*/

		// https://www.milanjovanovic.tech/blog/server-sent-events-in-aspnetcore-and-dotnet-10#consuming-server-sent-events-in-javascript
		static string j = "\n<script>"
+"\nconst source = new EventSource('SSE');"
+"\nconst msg = document.getElementById('msg');"
+"\nconst label = document.getElementById('active');"
+"\nconst slider = document.getElementById('myRange');"
+"\nconst table = document.getElementById('tok');"
+"\nlet rows = table.getElementsByTagName('tr');"

+"\nconst blurt = (string) => {"
+"	console.log(string); "
+"	msg.innerHTML = string;"
+"};"

+"\nconst tableUpdate = (data) => {"
+"\n  let obj = JSON.parse(data);"
+"\n  let r = obj.row;"
+"\n  let c = obj.col;"
+"\n  table.rows[r].cells[c].innerHTML = obj.val;"
+"\n};"

// Table Background Colors row, slider property name, value
+"\nfunction RowColor(r)\n{"
+"\n  for(i = 0; i < rows.length; i++)"
+"\n    rows[i].style.backgroundColor = (r == i) ? '#ffffff' : '#888888';\n}"

+"\nfunction RowColorSlider(r, slider, val)\n{"
+"\n  label.innerHTML = slider;"
+"\n  slider.value = val;"
+"\n  RowColor(r);\n}"

+"\nconst tableScroll = (data) => {"
+"\n  RowColor(JSON.parse(data).row); };"

+"\nconst slide = (data) => {"
+"\n  let obj = JSON.parse(data);"
+"\n  label.innerHTML = obj.prop;"
+"\n  slider.value = obj.val;"
+"\n};"

+CustomEvents  // SSEvents.cs

+"\nsource.onmessage = (event) => {"
+"\n  msg.innerHTML = event.data;"
+"\n  console.log('Received message:', event);"
+"\n};"

+"\nsource.onerror = (e) => {"
+"\n  let oops = 'Error: ' + e;"
+"\n  console.error(oops);"
+"\n  msg.innerHTML = oops;"
+"\n  if (source.readyState === EventSource.CONNECTING)"
+"\n	blurt('Reconnecting...');"
+"\n};"

+"\ndocument.addEventListener('DOMContentLoaded', function() {";

        internal static string JavaScript()
		{
			StringBuilder s = new StringBuilder(j);
			s.Append($"\n  RowColorSlider({1 + View.Selection}, '{SliderProperty}', {SliderValue});");
			s.Append("\n}, false);\n</script>");
			return s.ToString();
		}
	}		 // class
}			 // namespace
