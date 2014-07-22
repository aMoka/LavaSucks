using System;
using System.IO;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace LavaSucks
{
    [ApiVersion(1, 16)]
    public class LavaSucks : TerrariaPlugin
    {
		#region Info
        public override string Name
        {
            get { return "LavaSucks"; }
        }

        public override string Author
        {
            get { return "aMoka"; }
        }

        public override string Description
        {
            get { return "Autoremoves the blood of hellstone children during their murders."; }
        }

        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
        #endregion

        #region Initialize
        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
            }
            base.Dispose(disposing);
        }
        #endregion

        public LavaSucks(Main game)
            : base(game)
        {
            Order = 10;
        }

        #region OnGetData
        public static void OnGetData(GetDataEventArgs args)
        {
            if (!args.Handled && args.MsgID == PacketTypes.Tile)
            {
                using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    var action = reader.ReadByte();
                    var x = reader.ReadInt16();
                    var y = reader.ReadInt16();
                    var type = reader.ReadUInt16();

					//when this tile is destroyed...
                    if (Main.tile[x, y].type != 58) 
                        return;

                    if (action == 0)
                    {
						//remove the tile from play instead of sending to the graveyard...
						Main.tile[x, y].active(false);
                        Main.tile[x, y].frameX = -1;
                        Main.tile[x, y].frameY = -1;
                        Main.tile[x, y].liquidType(0);
                        Main.tile[x, y].liquid = 0;
                        Main.tile[x, y].type = 0;
                        TSPlayer.All.SendTileSquare(x, y);
						TShock.Players[args.Msg.whoAmI].SendTileSquare(x, y);

						//and special summon hellstone to the field
						Item itm = new Item();
						itm.SetDefaults(174);
						int itemid = Item.NewItem(x * 16, y * 16, itm.width, itm.height, itm.type, 1, true, 0, true);
						NetMessage.SendData((int)PacketTypes.ItemDrop, -1, -1, "", itemid, 0f, 0f, 0f);
                    }
                }
            }
        }
        #endregion
    }
}
