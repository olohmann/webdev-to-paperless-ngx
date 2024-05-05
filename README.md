# README

I run an [EPSON ES-C380W](https://www.epson.de/de_DE/produkte/scanner/business/es-c380w/p/40561) scanner at home to digitize incoming hard letters into my [Paperless-ngx](https://docs.paperless-ngx.com/) setup.

I don't like file shares and try to limit the usage as far as possible (even in my home network). So I decided to use the [Paperless-ngx API](https://docs.paperless-ngx.com/api/) to upload the scanned documents directly to my Paperless-ngx instance.

As the scanner is only able to either use a file share or a WebDAV share, I created a little adapter that emulates a WebDAV share and forwards the incoming requests to the Paperless-ngx API.

It is by absolute no means production-grade code, but it works for me. Feel free to use it, modify it, or do whatever you want with it.
