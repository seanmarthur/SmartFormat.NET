﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;



namespace StringFormatEx.Plugins
{
    public class _DefaultSourcePlugin : IStringFormatterPlugin
    {

        public IEnumerable<EventHandler<ExtendSourceEventArgs>> GetSourceExtensions()
        {
            return new EventHandler<ExtendSourceEventArgs>[]
                { _DefaultSourcePlugin._GetDefaultSource };
        }

        public IEnumerable<EventHandler<ExtendFormatEventArgs>> GetFormatExtensions()
        {
            return new EventHandler<ExtendFormatEventArgs>[] {};
        }


        /// <summary>
        /// This is the Default method for evaluating the Source.
        /// 
        /// 
        /// If this is the first selector and the selector is an integer, then it returns the (global) indexed argument (just like String.Format).
        /// If the Current item is a Dictionary that contains the Selectors, it returns the dictionary item.
        /// Otherwise, Reflection will be used to determine if the Selectors is a Selectors, Field, or ParseFormat of the Current item.
        /// </summary>
        [CustomFormatPriority(CustomFormatPriorities.Low)]
        private static void _GetDefaultSource(object source, ExtendSourceEventArgs e) 
        {
            ICustomSourceInfo info = e.SourceInfo;

            //  If it wasn't handled, let's evaluate the source on our own:
            //  We will see if it's an argument index, dictionary key, or a property/index/method.
            //  Maybe source is the global index of our arguments? 
            int argIndex;
            if (info.SelectorIndex == 0 && int.TryParse(info.Selector, out argIndex)) {
                if (argIndex < info.Arguments.Length) {
                    info.Current = info.Arguments[argIndex];
                }
                else {
                    //  The index is out-of-range!
                }
                return;
            }

            //  Maybe source is a Dictionary?
            if (info.Current is IDictionary && ((IDictionary)info.Current).Contains(info.Selector)) {
                info.Current = ((IDictionary)info.Current)[info.Selector];
                return;
            }


            // REFLECTION:
            // Let's see if the argSelector is a Selectors/Field/ParseFormat:
            var sourceType = info.Current.GetType();
            MemberInfo[] members = sourceType.GetMember(info.Selector);
            foreach (MemberInfo member in members) {
                switch (member.MemberType) {
                    case MemberTypes.Field:
                        //  Selectors is a Field; retrieve the value:
                        FieldInfo field = member as FieldInfo;
                        info.Current = field.GetValue(info.Current);
                        return;
                    case MemberTypes.Property:
                    case MemberTypes.Method:
                        MethodInfo method;
                        if (member.MemberType == MemberTypes.Property) {
                            //  Selectors is a Selectors
                            PropertyInfo prop = member as PropertyInfo;
                            //  Make sure the property is not WriteOnly:
                            if (prop.CanRead) {
                                method = prop.GetGetMethod();
                            }
                            else {
                                continue;
                            }
                        }
                        else {
                            //  Selectors is a ParseFormat
                            method = member as MethodInfo;
                        }

                        //  Check that this method is valid -- it needs to be a Function (return a value) and has to be parameterless:
                        //  We are only looking for a parameterless Selectors/ParseFormat:
                        if ((method.GetParameters().Length > 0)) {
                            continue;
                        }

                        //  Make sure that this method is not a Sub!  It has to be a Function!
                        if ((method.ReturnType == typeof(void))) {
                            continue;
                        }

                        //  Retrieve the Selectors/ParseFormat value:
                        info.Current = method.Invoke(info.Current, new object[0]);
                        return;
                }
            }
            //  If we haven't returned yet, then the item must be invalid.
        }

    }
}