

var activeNode;

Niviane = new Ext.Application({
    name: "Niviane",

    launch: function () {
        Niviane.defaultToolbar = new Ext.Toolbar({
            items: [{
                text: 'back'
				, title: 'Niviane HomeControl'
				, ui: 'back'
				, handler: function () {
				    Niviane.Viewport.setActiveItem('nodeList', { type: 'slide', direction: 'right' });
				}
            }]
        });

        Niviane.nodeList = new Ext.List({
            id: 'nodeList'
			, store: Niviane.nodeListStore
			, itemTpl: '<div class="contact">{Name} {Type}</div>'
            , listeners: {
                beforeactivate: function () {
                    if (Niviane.nodeListStore.data.length == 0) {
                        Ext.util.JSONP.request({
                            url: 'http://localhost:9087/Niviane/nodes',
                            callbackKey: 'callback',
                            callback: function (data) {
                                Niviane.nodeListStore.update(data);
                            }
                        });
                    }
                }
            }
            , onItemTap: function (item, index) {
                activeNode = Niviane.nodeList.getRecord(item).data
                Niviane.Viewport.setActiveItem(Niviane.nodeDetail);
            }
        });

        Niviane.nodeDetail = new Ext.Panel({
            id: 'nodeDetail'
            , store: Niviane.nodeStore
			, tpl: " "
            , items: [
                    {
                        xtype: 'panel'
                        , id: 'nodeDetails'
                        
                        , tpl: '<h2>{Name}</h2><p>{Location}</p><p>{Type}</p>'
                    },
					{
					    xtype: 'form'
						, title: 'Node'
						, instructions: 'Adjust the slider to set the basic level'
						, items: [
                            { xtype: 'sliderfield', id: 'basicLevel', name: 'basic', label: 'Basic', minValue: 0, maxValue: 100 }
                            , {
                                xtype: 'button'
									, name: 'submit'
									, text: 'Update'
									, ui: 'confirm'
									, width: '35%'
									, centered: 'true'
									, handler: function () {
									    Ext.util.JSONP.request({
									        url: 'http://localhost:9087/Niviane/node/' + activeNode.NodeID + '/basic/' + Niviane.nodeDetail.items.items[1].items.items[0].getValue(),
									        callbackKey: 'callback',
									        callback: function (data) {
									            //  Niviane.nodeStore.update(data);
									        }
									    });
									}
                            }

						]
					}
				]
            , listeners: {
                activate: function () {
                    Niviane.nodeDetail.items.items[0].update(activeNode);
                    if (activeNode.Type == "Binary Power Switch") {
                        Niviane.nodeDetail.items.items[1].items.items[0].maxValue = 255;
                    } else {
                        Niviane.nodeDetail.items.items[1].items.items[0].maxValue = 100;
                    }
                    Niviane.nodeDetail.items.items[1].items.items[0].setValue(activeNode.Basic);
                }
            }


        });

        Niviane.Viewport = new Ext.Panel({
            fullscreen: true,
            layout: "card",
            cardSwitchAnimation: "slide",
            items: [
                    Niviane.nodeList, Niviane.nodeDetail
                ]
			    , dockedItems: [Niviane.defaultToolbar]
        });
    }
});


/* Data stores */
Ext.regModel('Node', { fields: ['NodeID', 'Name', 'Location', 'Basic', 'Level', 'Type'] });

Niviane.nodeStore = new Ext.data.Store({
    model: 'Node'
    , data: []
});

Niviane.nodeListStore = new Ext.data.Store({
    model: 'Node'
    ,proxy: {
        type: 'scripttag',
        url: 'http://localhost:9087/Niviane/nodes'
        ,reader: {
            type: 'json'
        }
        ,writer: {
            type: 'json'
        }

    }
});

//    , data: [{ "Basic": 0, "Level": 0, "Location": "Bedroom", "Name": "Controller", "NodeID": 1, "Type": "Static PC Controller" }, { "Basic": 0, "Level": 19, "Location": "", "Name": "", "NodeID": 2, "Type": "Multilevel Switch" }, { "Basic": 0, "Level": 0, "Location": "", "Name": "", "NodeID": 3, "Type": "Binary Power Switch" }, { "Basic": 0, "Level": 0, "Location": "", "Name": "", "NodeID": 4, "Type": "Multilevel Power Switch"}]



