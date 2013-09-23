# linqpad-soap-driver

A SOAP service driver for LINQPad. Useful for invoking SOAP-based services with minimal pain and fuss. For the best experience, this goes especially well with the auto-complete support in [LINQPad Pro](http://www.linqpad.net/Purchase.aspx).

## setup

- Download the latest version [soap-driver-1.2.2.lpx](http://dylanmei.s3.amazonaws.com/linqpad-drivers/soap-driver-1.2.2.lpx). 
- In LINQPad load the driver by choosing:

   *Add Connection*  
   *View more drivers*  
   *Browse to a .LPX file*

- Complete the connection dialog by entering the HTTP URL of your service and then selecting the SOAP binding you wish to use.
- Reveal the endpoint operations by expanding your new connection in the LINQPad explorer view.

## troubleshooting
* When connecting, the driver is looking for the service's WSDL file by looking in common locations. If you're having trouble here, try specifying the exact HTTP URL of the WSDL file as you'd see it in your browser.
* .NET DateTime fields don't work very well.
* Nullable fields don't work very well. For such fields, an extra *Specified* field is generated. You must set this extra field as *true* if you are specifying a value for your Nullable field.

## contributing

1. Fork it
2. Create your feature branch `git checkout -b my-new-feature`
3. Commit your changes `git commit -am 'Added some feature'`
4. Push to the branch `git push origin my-new-feature`
5. Create new Pull Request
