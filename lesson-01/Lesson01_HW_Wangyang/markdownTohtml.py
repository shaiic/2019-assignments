import codecs
import markdown
from mdx_math import MathExtension


def genhtml(text, scripts, html_file):
    html_content = markdown.markdown(text, extensions=[MathExtension(enable_dollar_delimiter=True)])
    html_file.write(html_content)

    html_script = markdown.markdown(scripts)
    html_file.write(html_script)


def main():
    text_file = codecs.open("./text.md", mode='r', encoding="utf-8")
    text = text_file.read()
    text_file.close()

    scripts_file = codecs.open("./html_script", mode='r', encoding="utf-8")
    scripts = scripts_file.read()
    scripts_file.close()

    html_file = codecs.open("./text.html", mode='w', encoding="utf-8")
    genhtml(text, scripts, html_file)
    html_file.close()


if __name__ == "__main__":
    main()
