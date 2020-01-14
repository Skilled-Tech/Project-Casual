namespace MyJSON
{
    export const IgnoreCharacter = '$';

    export function Write(object: any): string
    {
        function Replacer(key: string, value: any)
        {
            if (key[0] == IgnoreCharacter) return undefined;

            return value;
        }

        var json = JSON.stringify(object, Replacer);

        return json;
    }

    export function Read<TObject>(constructor: new (instance: TObject) => TObject, json: string): TObject
    {
        var object = <TObject>JSON.parse(json);

        var instance = new constructor(object);

        return instance;
    }
}

namespace Utility
{
    export namespace Dates
    {
        export function DaysFrom(date: any): number
        {
            return DaysBetween(date, new Date());
        }

        export function DaysBetween(date1: Date, date2: Date): number
        {
            return Math.round((date2.valueOf() - date1.valueOf()) / (86400000));
        }
    }

    export namespace Class
    {
        export function Assign<T>(ctor: new () => T, props: Partial<T>): T
        {
            return Object.assign(new ctor(), props);
        }

        export function WriteProperty(target: any, name: string, value: any)
        {
            let descriptor: PropertyDescriptor = {
                value: value,
                enumerable: true,
                writable: false,
            }

            Object.defineProperty(target, name, descriptor);
        }
    }
}