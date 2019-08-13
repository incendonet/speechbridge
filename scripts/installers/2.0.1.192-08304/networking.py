#
# networking.py - networking module for firstboot
#
# Copyright 2002, 2003 Red Hat, Inc.
# Copyright 2002, 2003 Brent Fox <bfox@redhat.com>
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
#

from gtk import *
import string
import os
import time
import gtk
import gobject
import sys
import functions
import libuser
import kudzu
from firstboot import start_process

##
## I18N
## 
from rhpl.translate import _, N_
from rhpl import translate
translate.textdomain("firstboot")
class childWindow:
    #You must specify a runPriority for the order in which you wish your module to run
    runPriority = 45
    moduleName = (_("Network Setup"))
    moduleClass = "reconfig"

    def launch(self, doDebug = None):
        self.doDebug = doDebug
        self.admin = libuser.admin()

        if doDebug:
            print "initializing networking module"

        self.usernameEntry = gtk.Entry()
        self.fullnameEntry = gtk.Entry()
        self.passwordEntry = gtk.Entry()
        self.passwordEntry.set_visibility(False)
        self.confirmEntry = gtk.Entry()
        self.confirmEntry.set_visibility(False)
        
        self.vbox = gtk.VBox()
        self.vbox.set_size_request(400, 200)

        title_pix = functions.imageFromFile("networking.png")

        internalVBox = gtk.VBox()
        internalVBox.set_border_width(10)
        internalVBox.set_spacing(10)

        label = gtk.Label(_("The devices listed below have been detected on the system."
                            "You must change the configuration for 'eth0' from DHCP to static-IP addressing (or reserve the IP address in your DHCP server.)  "
                            "Once you have saved your static IP, you will need to click 'Deactivate', then 'Activate', and close the Networking window.  "
                            "Then click 'Change...' again and set IP address for your DNS server.  (This extra step will ensure that the DNS setting is properly saved.) "
                            ""))

        label.set_line_wrap(True)
        label.set_alignment(0.0, 0.5)
        label.set_size_request(500, -1)
        internalVBox.pack_start(label, False, True)

        self.deviceStore = gtk.ListStore(gobject.TYPE_STRING, gobject.TYPE_STRING)
        self.deviceView = gtk.TreeView()
        self.deviceView.set_size_request(-1, 200)
        self.deviceView.set_model(self.deviceStore)
        self.deviceSW = gtk.ScrolledWindow()
        self.deviceSW.set_policy(gtk.POLICY_AUTOMATIC, gtk.POLICY_AUTOMATIC)
        self.deviceSW.set_shadow_type(gtk.SHADOW_IN)
        self.deviceSW.add(self.deviceView)

        self.deviceBox = gtk.HBox()
        self.deviceBox.pack_start(self.deviceSW, True)
        
        col = gtk.TreeViewColumn(_("Network Device"), gtk.CellRendererText(), text = 0)
        self.deviceView.append_column(col)
        col = gtk.TreeViewColumn(_("Boot protocol"), gtk.CellRendererText(), text = 1)        
        self.deviceView.append_column(col)
        
        internalVBox.pack_start(self.deviceBox, False)
        self.updateLabels()

        networkButton = gtk.Button(_("_Change Network Configuration..."))
        networkButton.connect("clicked", self.run_neat)

        align = gtk.Alignment()
        align.add(networkButton)
        align.set(1.0, 0.5, 0.1, 1.0)
                             
        internalVBox.pack_start(align, False)

        self.vbox.pack_start(internalVBox, False, 15)

        users = self.admin.enumerateUsersFull()
        self.normalUsersList = []
        for userEnt in users:
            uidNumber = int(userEnt.get(libuser.UIDNUMBER)[0])
            if uidNumber == 500:
                self.usernameEntry.set_text(userEnt.get(libuser.USERNAME)[0])
                self.fullnameEntry.set_text(userEnt.get(libuser.GECOS)[0])

        return self.vbox, title_pix, self.moduleName

    def updateLabels(self):
        module_dict = {}
        lines = open("/etc/modprobe.conf").readlines()
        for line in lines:
            #  Skip empty lines and comments.
            if line.isspace() or (line != "" and line.lstrip()[0] == '#'):
                continue

            tokens = string.split(line)

            # If the line isn't formed just like we like it, don't explode.
            try:
                if string.strip(tokens[0]) == "alias" and \
                   string.strip(tokens[1][:3]) == "eth":
                    module_dict[tokens[1]] = tokens[2]
            except IndexError:
                pass

        self.deviceStore.clear()
        
        for dev in module_dict.keys():
            path = '/etc/sysconfig/network-scripts/ifcfg-%s' % dev
            try:
                lines = open(path).readlines()
            except:
                continue
            bootproto = None
            for line in lines:
                if string.strip(line[:9]) == "BOOTPROTO":
                    key, value = string.split(line, "=")
                    key = string.strip(key)
                    value = string.strip(string.lower(value))
                    if value == "none":
                        iter = self.deviceStore.append()
                        self.deviceStore.set_value(iter, 0, dev)
                        self.deviceStore.set_value(iter, 1, (_("static")))
                    else:
                        iter = self.deviceStore.append()
                        self.deviceStore.set_value(iter, 0, dev)
                        self.deviceStore.set_value(iter, 1, value)

    def apply(self, notebook):
        return 0

    def run_neat(self, *args):
        #Create a gtkInvisible dialog to block until up2date is complete
        i = gtk.Invisible ()
        i.grab_add ()

        #Run rhn_register so they can register with RHN
        pid = start_process("/usr/bin/system-config-network")

        flag = None
        while not flag:
            while gtk.events_pending():
                gtk.main_iteration_do()

            child_pid, status = os.waitpid(pid, os.WNOHANG)
            
            if child_pid == pid:
                flag = 1
            else:
                time.sleep(0.1)

        i.grab_remove ()
        self.updateLabels()
