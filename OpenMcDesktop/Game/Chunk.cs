using OpenMcDesktop.Game.Definitions;
using OpenMcDesktop.Networking;
using SFML.Window;

namespace OpenMcDesktop.Game;

public class Chunk
{
	public int X;
	public int Y;
	// The blocks present within this chunk
	public List<int> Tiles;
	// A palette of all block types that are used in this chunk
	public List<Type> Palette;
	// A list of all entities belonging/within this chunk
	public List<Entity> Entities;
	public byte[] Biomes;

	public Chunk(ReadablePacket data, GameData gameData)
	{
		var x = data.ReadInt();
		var y = data.ReadInt();

		X = x << 6 >> 6;
		Y = y << 6 >> 6;
		Tiles = new List<int>();
		Entities = new List<Entity>();
		Palette = new List<Type>();

		// Read buffer palette
		var paletteLength = (x >>> 26) + (y >>> 26) * 64 + 1;
		
		// Read and add all entities belonging to this chunk
		var entityId = data.ReadShort();
		while (entityId != 0) // ()
		{
			var entity = Activator.CreateInstance(gameData.EntityDefinitions[entityId],
				new[] {data.ReadShort() / 1024 + (x << 6), data.ReadShort() / 1024 + (y << 6)}) as Entity;

			if (entity is null)
			{
				continue;
			}

			entity.Id = data.ReadUInt() + data.ReadUShort() * int.MaxValue;
			entity.Name = data.ReadString();
			entity.State = data.ReadShort();
			entity.Displacement.X = data.ReadFloat();
			entity.Displacement.Y = data.ReadFloat();
			entity.Facing = data.ReadDouble();
			entity.Age = data.ReadDouble();
			entity.Chunk = this;
			
			// We add all entities back to the global world
			World.AddEntity(entity);
			Entities.Add(entity);
			entityId = data.ReadShort();
		}

		Biomes = new[]
		{
			data.ReadByte(), data.ReadByte(), data.ReadByte(), data.ReadByte(), data.ReadByte(), data.ReadByte(),
			data.ReadByte(), data.ReadByte(), data.ReadByte(), data.ReadByte()
		};

		for (var i = 0; i < paletteLength; i++)
		{
			Palette.Add(gameData.BlocksDefinitions[data.ReadShort()]);
		}
	}

	public void Render(Window window)
	{
		
	}
}
/*
import { addEntity } from 'world'
import { BlockIDs, EntityIDs } from 'definitions'
const canvasPool = []
export class Chunk{
	constructor(buf){
		const x = buf.int(), y = buf.int()
		this.x = x << 6 >> 6
		this.y = y << 6 >> 6
		this.tiles = []
		this.entities = new Set()
		this.ctx = null
		this.r1 = 0
		this.r2 = 0
		//read buffer palette
		let palettelen = (x >>> 26) + (y >>> 26) * 64 + 1
		let id = buf.short()
		while(id){
			const e = EntityIDs[id](buf.short() / 1024 + (this.x << 6), buf.short() / 1024 + (this.y << 6))
			e._id = buf.uint32() + buf.uint16() * 4294967296
			e.name = buf.string(); e.state = buf.short()
			e.dx = buf.float(); e.dy = buf.float()
			e.f = buf.float(); e.age = buf.double()
			e.chunk = this
			if(e.savedata)buf.read(e.savedatahistory[buf.flint()] || e.savedata, e)
			addEntity(e)
			this.entities.add(e)
			if(e.placed)e.placed()
			id = buf.short()
		}
		this.biomes = [data.ReadByte(), data.ReadByte(), data.ReadByte(), data.ReadByte(), data.ReadByte(), data.ReadByte(), data.ReadByte(), data.ReadByte(), data.ReadByte(), data.ReadByte()]
		let palette = []
		let i = 0
		for(;i<palettelen;i++){
			palette.push(BlockIDs[buf.short()])
		}
		let j = 0; i = 11 + i * 2
		if(palettelen<2){
			for(;j<4096;j++)this.tiles.push(palette[0])
		}else if(palettelen == 2){
			for(;j<512;j++){
				const byte = data.ReadByte()
				this.tiles.push(palette[byte&1])
				this.tiles.push(palette[(byte>>1)&1])
				this.tiles.push(palette[(byte>>2)&1])
				this.tiles.push(palette[(byte>>3)&1])
				this.tiles.push(palette[(byte>>4)&1])
				this.tiles.push(palette[(byte>>5)&1])
				this.tiles.push(palette[(byte>>6)&1])
				this.tiles.push(palette[byte>>7])
			}
		}else if(palettelen <= 4){
			for(;j<1024;j++){
				const byte = data.ReadByte()
				this.tiles.push(palette[byte&3])
				this.tiles.push(palette[(byte>>2)&3])
				this.tiles.push(palette[(byte>>4)&3])
				this.tiles.push(palette[byte>>6])
			}
		}else if(palettelen <= 16){
			for(;j<2048;j++){
				const byte = data.ReadByte()
				this.tiles.push(palette[byte&15])
				this.tiles.push(palette[(byte>>4)])
			}
		}else if(palettelen <= 256){
			for(;j<4096;j++){
				this.tiles.push(palette[data.ReadByte()])
			}
		}else{
			for(;j<6144;j+=3){
				let byte2
				this.tiles.push(palette[data.ReadByte() + (((byte2 = data.ReadByte())&0x0F)<<8)])
				this.tiles.push(palette[data.ReadByte() + ((byte2&0xF0)<<4)])
			}
		}
		//parse block entities
		for(j=0;j<4096;j++){
			const block = this.tiles[j]
			if(!block){this.tiles[j] = Blocks.air; continue}
			if(!block.savedata)continue
			//decode data
			this.tiles[j] = buf.read(block.savedatahistory[buf.flint()] || block.savedata, block())
		}
	}
	hide(){
		if(!this.ctx)return
		canvasPool.push(this.ctx)
		this.ctx.clearRect(0, 0, TEX_SIZE << 6, TEX_SIZE << 6)
		this.ctx = null
	}
	draw(){
		if(this.ctx)return
		this.ctx = canvasPool.pop()
		if(!this.ctx)this.ctx = Can(TEX_SIZE << 6, TEX_SIZE << 6)
		for(let x = 0; x < 64; x++){
			for(let y = 0; y < 64; y++){
				const t = this.tiles[x|(y<<6)].texture
				if(!t)continue
				this.ctx.drawImage(t.canvas,t.x,t.y,t.w,t.h,x*TEX_SIZE,(63-y)*TEX_SIZE,TEX_SIZE,TEX_SIZE)
			}
		}
	}
}
 */