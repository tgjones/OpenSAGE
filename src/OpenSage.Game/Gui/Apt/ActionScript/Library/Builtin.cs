using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenSage.Gui.Apt.ActionScript.Library
{
    /// <summary>
    /// This class is meant to access builtin variables and return the corresponding value
    /// </summary>
    public static class Builtin
    {
        private static readonly Dictionary<string, Func<Value[], Value>> BuiltinClasses;
        private static readonly Dictionary<string, Action<ActionContext, ObjectContext, Value[]>> BuiltinFunctions;
        private static readonly Dictionary<string, Func<ObjectContext, Value>> BuiltinVariablesGet;
        private static readonly Dictionary<string, Action<ObjectContext, Value>> BuiltinVariablesSet;
        public static DateTime InitTimeStamp { get; } = DateTime.Now;

        static Builtin()
        {
            // list of builtin objects and their corresponding constructors
            BuiltinClasses = new Dictionary<string, Func<Value[], Value>>
            {
                ["Color"] = args => Value.FromObject(new ASColor()),
                ["Array"] = args => Value.FromObject(new ASArray(args)),
                ["Object"] = args => Value.FromObject(new ObjectContext()),
                ["Function"] = args => throw new NotImplementedException("Nonetheless this is not the real ActionScript. "),
            };

            // list of builtin variables
            BuiltinVariablesGet = new Dictionary<string, Func<ObjectContext, Value>>
            {
                // Globals
                ["_root"] = ctx => Value.FromObject(ctx.Item.Context.Root.ScriptObject),
                ["_global"] = ctx => Value.FromObject(ctx.Item.Context.Avm.GlobalObject),
                ["extern"] = ctx => Value.FromObject(ctx.Item.Context.Avm.ExternObject),
                // Object specifc
                ["_parent"] = GetParent,
                ["_name"] = ctx => Value.FromString(ctx.Item.Name),
                ["_x"] = GetX,
                ["_y"] = GetY,
                ["_currentframe"] = ctx => Value.FromInteger(((SpriteItem) ctx.Item).CurrentFrame),
            };

            // list of builtin variables - set
            BuiltinVariablesSet = new Dictionary<string, Action<ObjectContext, Value>>
            {
                ["_alpha"] = (ctx, v) =>
                {
                    var transform = ctx.Item.Transform;
                    ctx.Item.Transform =
                        transform.WithColorTransform(transform.ColorTransform.WithA(v.ToInteger() / 100.0f));
                },
                ["textColor"] = (ctx, v) =>
                {
                    var hexStr = v.ToString();
                    var hexColor = Convert.ToInt32(hexStr, 16);

                    var b = (hexColor & 0xFF) / 255.0f;
                    var g = ((hexColor & 0xFF00) >> 8) / 255.0f;
                    var r = ((hexColor & 0xFF0000) >> 16) / 255.0f;

                    var transform = ctx.Item.Transform;
                    ctx.Item.Transform =
                        transform.WithColorTransform(transform.ColorTransform.WithRGB(r, g, b));
                },
            };

            // list of builtin functions
            BuiltinFunctions = new Dictionary<string, Action<ActionContext, ObjectContext, Value[]>>
            {
                ["gotoAndPlay"] = (actx, ctx, args) => GotoAndPlay(actx, ctx, args),
                ["gotoAndStop"] = (actx, ctx, args) => GotoAndStop(ctx, args),
                ["stop"] = (actx, ctx, args) => Stop(ctx),
                ["clearInterval"] = ClearInterval,
                ["setInterval"] = SetInterval,
                ["loadMovie"] = LoadMovie,
                // Global constructors / functions
                ["Boolean"] = BoolFunc,
                ["attachMovie"] = AttachMovie,
                ["getTime"] = (actx, ctx, args) => GetTime(actx),
                
            };
        }

        public static bool IsBuiltInClass(string name)
        {
            return BuiltinClasses.ContainsKey(name);
        }

        public static bool IsBuiltInFunction(string name)
        {
            return BuiltinFunctions.ContainsKey(name);
        }

        public static bool IsBuiltInVariable(string name)
        {
            return BuiltinVariablesGet.ContainsKey(name) || BuiltinVariablesSet.ContainsKey(name);
        }

        public static void CallBuiltInFunction(string name, ActionContext actx, ObjectContext ctx, Value[] args)
        {
            BuiltinFunctions[name](actx, ctx, args);
        }

        public static Value GetBuiltInVariable(string name, ObjectContext ctx)
        {
            return BuiltinVariablesGet[name](ctx);
        }

        public static void SetBuiltInVariable(string name, ObjectContext ctx, Value val)
        {
            BuiltinVariablesSet[name](ctx, val);
        }

        public static Value GetBuiltInClass(string name, Value[] args)
        {
            return BuiltinClasses[name](args);
        }

        private static Value GetX(ObjectContext ctx)
        {
            return Value.FromFloat(ctx.Item.Transform.GeometryTranslation.X);
        }

        private static Value GetY(ObjectContext ctx)
        {
            return Value.FromFloat(ctx.Item.Transform.GeometryTranslation.Y);
        }

        private static Value GetParent(ObjectContext ctx)
        {
            var item = ctx.Item;

            // Parent of a render item is the parent of the containing sprite
            // TODO: By doing some search on the web,
            // it seems like in Flash / ActionScript 3, when trying to access
            // the `parent` of root object, null or undefined will be returned.
            var parent = item is RenderItem ? item.Parent?.Parent?.ScriptObject : item.Parent?.ScriptObject;
            return Value.FromObject(parent);
        }

        private static void GotoAndPlay(ActionContext actx, ObjectContext ctx, Value[] args)
        {
            if (ctx.Item is SpriteItem si)
            {
                var dest = args.First().ResolveRegister(actx);

                if (dest.Type == ValueType.String)
                {
                    si.Goto(dest.ToString());
                }
                else if (dest.Type == ValueType.Integer)
                {
                    si.GotoFrame(dest.ToInteger() - 1);
                }
                else
                {
                    throw new InvalidOperationException("Can only jump to labels or frame numbers");
                }

                si.Play();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static void GotoAndStop(ObjectContext ctx, Value[] args)
        {
            if (ctx.Item is SpriteItem si)
            {
                var dest = args.First();

                if (dest.Type == ValueType.String)
                {
                    si.Goto(dest.ToString());
                }
                else if (dest.Type == ValueType.Integer)
                {
                    si.GotoFrame(dest.ToInteger() - 1);
                }
                else
                {
                    throw new InvalidOperationException("Can only jump to labels or frame numbers");
                }

                si.Stop(true);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static void Stop(ObjectContext ctx)
        {
            if (ctx.Item is SpriteItem si)
            {
                si.Stop();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }


        private static void GetTime(ActionContext context)
        {
            var result_ = DateTime.Now - Builtin.InitTimeStamp;
            var result = Value.FromFloat(result_.TotalMilliseconds);
            context.Push(result);
        }

        private static void LoadMovie(ActionContext context, ObjectContext ctx, Value[] args)
        {
            var url = Path.ChangeExtension(args[0].ToString(), ".apt");
            var window = context.Apt.Window.Manager.Game.LoadAptWindow(url);

            context.Apt.Window.Manager.QueryPush(window);
        }

        private static void AttachMovie(ActionContext context, ObjectContext ctx, Value[] args)
        {
            var url = Path.ChangeExtension(args[0].ToString(), ".apt");
            var name = args[1].ToString();
            var depth = args[2].ToInteger();
        }

        private static void SetInterval(ActionContext context, ObjectContext ctx, Value[] args)
        {
            var vm = context.Apt.Avm;
            var name = context.Pop().ToString();

            vm.CreateInterval(name, args[1].ToInteger(), args[0].ToFunction(), ctx, Array.Empty<Value>());

            ctx.Variables[name] = Value.FromString(name);
        }

        private static void ClearInterval(ActionContext context, ObjectContext ctx, Value[] args)
        {
            var vm = context.Apt.Avm;
            var name = args[0].ToString();

            vm.ClearInterval(name);
            ctx.Variables.Remove(name);
        }

        private static void BoolFunc(ActionContext context, ObjectContext ctx, Value[] args)
        {
            var result = Value.FromBoolean(args[0].ToBoolean());
            context.Push(result);
        }

    }
}
