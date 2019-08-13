from gtk import *
import string
import gtk
import gobject
import sys
import functions
import os

##
## I18N
## 
from rhpl.translate import _, N_
from rhpl import translate
translate.textdomain("firstboot")

class childWindow:
    #You must specify a runPriority for the order in which you wish your module to run
    runPriority = 499
    moduleName = (_("Setup Complete"))

    def launch(self, doDebug = None):
        if doDebug:
            print "initializing sbcomplete module"

        self.vbox = gtk.VBox()
        self.vbox.set_size_request(400, 200)

        msg = (_("Setup Complete"))

        title_pix = functions.imageFromFile("workstation.png")

        internalVBox = gtk.VBox()
        internalVBox.set_border_width(10)

        label = gtk.Label(_("Your SpeechBridge setup is almost complete, but you still need to install your license key(s) and configure SpeechBridge from the administration website.  "
                          "If you are installing a SpeechBridge Pro system, or are including the TTS upgrade, you will need to log in and run the following command from a terminal:\n    /home/speechbridge/software/neoinstall.sh\n"
#                          "After you click 'Finish', click the \"Restart\" link at the bottom of the login screen in order to make sure all of the settings have taken effect.  "
#                          "After the system reboots, you will be taken to the login screen where "
                          "After you click 'Finish' you will be taken to the login screen where "
                          "you can start the SpeechBridge web administration "
                          "site by launching Firefox and going to \"https://localhost\", or "
                          "you can also start it from a browser on another machine by replacing "
                          "\"localhost\" with the IP address you specified in the network configuration earlier.  Enjoy! "
                          ""))

        label.set_line_wrap(True)
        label.set_alignment(0.0, 0.5)
        label.set_size_request(500, -1)
        internalVBox.pack_start(label, False, True)

        self.vbox.pack_start(internalVBox, False, 5)
        pix = functions.ditheredImageFromFile("splash-small.png")
        self.vbox.pack_start(pix, True, True, 5)

        return self.vbox, title_pix, msg

    def apply(self, notebook):
        return 0
