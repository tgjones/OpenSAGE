using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript.Library;

namespace OpenSage.Gui.Apt.ActionScript
{
    public class ObjectContext
    {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ObjectContext __proto__
        {
            get { return GetMember("__proto__").ToObject(); }
            set { SetMember("__proto__", Value.FromObject(value)); }
        }
        public ObjectContext prototype
        {
            get { return GetMember("prototype").ToObject(); }
            set { SetMember("prototype", Value.FromObject(value)); }
        }
        public Function constructor
        {
            get { return GetMember("constructor").ToFunction(); }
            set { SetMember("constructor", Value.FromFunction(value)); }
        }

        /// <summary>
        /// The item that this context is connected to
        /// </summary>
        public DisplayItem Item { get; private set; }

        /// <summary>
        /// Contains functions and member variables
        /// </summary>
        public Dictionary<string, Value> Variables { get; set; }

        // public List<Value> Constants { get; set; }

        /// <summary>
        /// this ActionScript object is not bound bound to an item, e.g. for global object
        /// </summary>
        public ObjectContext()
        {
            //Actionscript variables are not case sensitive!
            Variables = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
            // Constants = new List<Value>();

            __proto__ = ObjectPrototype;
        }

        internal protected ObjectContext(bool JustUsedToCreateObjectPrototype)
        {
            Variables = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
            // Constants = new List<Value>();

            __proto__ = null;
        }

        /// <summary>
        /// this ActionScript object is bound to an item
        /// </summary>
        /// <param name="item"></param>
        /// the item that this context is bound to
        public ObjectContext(DisplayItem item) : this()
        {
            Item = item;

            //initialize item dependent properties
            InitializeProperties();
        }

        /// <summary>
        /// return a variable, when not present return Undefined
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public virtual Value GetMember(string name)
        {
            if (IsBuiltInVariable(name))
            {
                return GetBuiltInVariable(name);
            }

            if (Variables.TryGetValue(name, out var result))
            {
                return result;
            }

            logger.Warn($"[WARN] Undefined variable: {name}");
            return Value.Undefined();
        }


        public virtual void SetMember(string name, Value val)
        {
            logger.Warn("Not comprehensive function: SetMember");
            if (IsBuiltInVariable(name))
            {
                SetBuiltInVariable(name, val);
            }
            Variables[name] = val;
        }

        public virtual bool HasMember(string name)
        {
            if (IsBuiltInVariable(name))
            {
                throw new NotImplementedException();
            }
            else
            {
                return Variables.ContainsKey(name);
            }
            
        }

        public virtual void DeleteMember(string name)
        {
            if (IsBuiltInVariable(name))
            {
                throw new NotImplementedException();
            }
            else
            {
                if (HasMember(name)) Variables.Remove(name);
            }
        }

        /// <summary>
        /// Check wether or not a string is a builtin flash variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public virtual bool IsBuiltInVariable(string name)
        {
            if (name == "__proto__" || name == "prototype" || name == "constructor")
                return false;
            return Builtin.IsBuiltInVariable(name);
        }

        /// <summary>
        /// Get builtin variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public virtual Value GetBuiltInVariable(string name)
        {
            return Builtin.GetBuiltInVariable(name, this);
        }

        /// <summary>
        /// Set a builtin flash variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public virtual void SetBuiltInVariable(string name, Value val)
        {
            Builtin.SetBuiltInVariable(name, this, val);
        }

        /// <summary>
        /// Check whether or not a string is a builtin flash function
        /// </summary>
        /// <param name="name">function name</param>
        /// <returns></returns>
        public virtual bool IsBuiltInFunction(string name)
        {
            return Builtin.IsBuiltInFunction(name);
        }

        /// <summary>
        /// Execute a builtin function maybe move builtin functions elsewhere
        /// </summary>
        /// <param name="actx"></param>
        /// <param name="name">function name</param>
        /// <param name="args"></param>
        public virtual void CallBuiltInFunction(ActionContext actx, string name, Value[] args)
        {
            Builtin.CallBuiltInFunction(name, actx, this, args);
        }
        
        private static ObjectContext _createPrototypes()
        {
            var _op = new ObjectContext(false);
            FunctionPrototype = new ObjectContext(false);
            _foc = new Function(false);
            _ffc = new Function(false);

            FunctionPrototype.constructor = _ffc;
            FunctionPrototype.__proto__ = _op;
            _foc.__proto__ = FunctionPrototype;
            _ffc.__proto__ = FunctionPrototype;
            _foc.prototype = _op;
            _ffc.prototype = FunctionPrototype;

            _op.constructor = _foc;
            return _op;
        }

        // class inheritance
        protected internal static Function _foc { get; private set; }
        protected internal static Function _ffc { get; private set; }
        public static readonly ObjectContext ObjectPrototype = _createPrototypes(); // = new ObjectContext(false) { __proto__ = null, constructor = Function.ObjectConstructor };
        public static ObjectContext FunctionPrototype { get; private set; } // = new ObjectContext() { constructor = Function.FunctionConstructor };

        public bool IsFunction()  { return this is Function || __proto__ == FunctionPrototype;  }
        public bool IsConstructor() { return IsFunction() && prototype.IsPrototype(); }
        public bool IsPrototype() { return constructor != null && constructor.IsFunction(); }

        public bool InstanceOf(ObjectContext cst) // TODO Not complete
        {
            return cst.IsFunction() && (cst.prototype == this.__proto__);
        }

        // properties

        private void InitializeProperties()
        {
            //TODO: avoid new fancy switch
            switch (Item.Character)
            {
                case Text t:
                    Variables["textColor"] = Value.FromString(t.Color.ToHex());
                    break;
            }
        }

        /// <summary>
        /// used by text
        /// </summary>
        /// <param name="value">value name</param>
        /// <returns></returns>
        public Value ResolveValue(string value, ObjectContext ctx)
        {
            var path = value.Split('.');
            var obj = ctx.GetParent();
            var member = path.Last();

            for (var i = 0; i < path.Length - 1; i++)
            {
                var fragment = path[i];

                if (Builtin.IsBuiltInVariable(fragment))
                {
                    obj = Builtin.GetBuiltInVariable(fragment, obj).ToObject();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return obj.GetMember(member);
        }

        public ObjectContext GetParent()
        {
            ObjectContext result = null;

            if (Item.Parent != null)
            {
                result = Item.Parent.ScriptObject;
            }

            return result;
        }

        public Value GetProperty(PropertyType property)
        {
            Value result = null;

            switch (property)
            {
                case PropertyType.Target:
                    result = Value.FromString(GetTargetPath());
                    break;
                case PropertyType.Name:
                    result = Value.FromString(Item.Name);
                    break;
                case PropertyType.X:
                    result = Value.FromFloat(Item.Transform.GeometryTranslation.X);
                    break;
                case PropertyType.Y:
                    result = Value.FromFloat(Item.Transform.GeometryTranslation.Y);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        public void SetProperty(PropertyType property, Value val)
        {
            switch (property)
            {
                case PropertyType.Visible:
                    Item.Visible = val.ToBoolean();
                    break;
                case PropertyType.XScale:
                    Item.Transform.Scale((float) val.ToFloat(), 0.0f);
                    break;
                case PropertyType.YScale:
                    Item.Transform.Scale(0.0f, (float) val.ToFloat());
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Calculates the absolute target path
        /// </summary>
        /// <returns>the target</returns>
        private string GetTargetPath()
        {
            string path;

            if (GetParent() == null)
                path = "/";
            else
            {
                path = GetParent().GetTargetPath();
                path += Item.Name;
            }

            return path;
        }

        public override string ToString()
        {
            return Item == null ? "null item" : Item.Name;
        }

        public Value ToPrimitive()
        {
            return null;
        }

        public string ToStringDisp()
        {
            string ans = "{\n";
            foreach (string s in Variables.Keys)
            { 
                Variables.TryGetValue(s, out var v);
                ans = ans + s + ": " + v + ", \n";
            }
            ans = ans + "}";
            return ans;
        }
    }
}
